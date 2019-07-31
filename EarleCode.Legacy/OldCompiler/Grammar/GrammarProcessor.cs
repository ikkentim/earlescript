// EarleCode
// Copyright 2016 Tim Potze
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using EarleCode.OldCompiler.Lexing;

namespace EarleCode.OldCompiler.Grammar
{
	public class GrammarProcessor
	{
		private readonly GrammarRuleTable _table = new GrammarRuleTable();

		public void AddRule(string name, bool isStatement, string rule)
		{
			if (name == null)
				throw new ArgumentNullException(nameof(name));
			if (rule == null)
				throw new ArgumentNullException(nameof(rule));

			_table.Add(new GrammarRule(name, isStatement, Compile(rule)));
		}

		public void AddRules(string name, bool isStatement, IEnumerable<string> rules)
		{
			if (name == null)
				throw new ArgumentNullException(nameof(name));
			if (rules == null)
				throw new ArgumentNullException(nameof(rules));

			foreach (var rule in rules)
				AddRule(name, isStatement, rule);
		}

		public string GetMatch(ILexer lexer, bool onlyStatements = false)
		{
			foreach (var ruleName in _table.Where(rule => !onlyStatements || rule.IsStatement).Select(r => r.Name).Distinct())
			{
				if (IsMatch(lexer, ruleName))
				{
					return ruleName;
				}
			}

			return null;
		}

		public bool IsMatch(ILexer lexer, string ruleName)
		{
			return GetMatches(lexer, ruleName).Any();
		}

		public bool MatchStartsWith(ILexer lexer, string ruleNameStartWith)
		{
			return _table.Where(r => r.Name.StartsWith(ruleNameStartWith, StringComparison.Ordinal))
				.Select(r => r.Name)
				.Distinct()
				.Any(n => IsMatch(lexer, n));
		}

		public IEnumerable<ILexer> GetMatches(ILexer lexer, string ruleName)
		{
			var wrapRules = new List<GrammarRule>();
			var results = new List<ILexer>();

			foreach (var rule in _table.Get(ruleName))
			{
				if (rule.Elements[0] is GrammarRuleElementEmbed && ((GrammarRuleElementEmbed) rule.Elements[0]).RuleName == ruleName)
				{
					if (rule.Elements.Length > 1)
						wrapRules.Add(rule);
				}
				else
				{
					foreach (var match in GetMatches(lexer, rule))
					{
						yield return match;
						results.Add(match);
					}
				}
			}

			foreach (var start in results)
				foreach (var match in MatchesRecursive(start, wrapRules))
					yield return match;
		}

		private IEnumerable<ILexer> MatchesRecursive(ILexer lexer, IEnumerable<GrammarRule> rules)
		{
			var last = new List<ILexer>(new[] {lexer});
			for (;;)
			{
				var newLast = new List<ILexer>();

				foreach (var l in last)
					foreach (var rule in rules)
						foreach (var match in GetMatches(l, rule, 1))
						{
							yield return match;
							newLast.Add(match);
						}


				if (!newLast.Any())
					yield break;

				last = newLast;
			}
		}

		private IEnumerable<ILexer> GetMatches(ILexer lexer, GrammarRule rule, int startElement = 0)
		{
			if (lexer.Current == null)
				yield break;

			var element = rule.Elements[startElement];

			if (rule.Elements.Length - 1 == startElement)
			{
				// Last element
				foreach (var match in element.Match(this, lexer))
					yield return match;
			}
			else
			{
				// Not last element
				foreach (var match in element.Match(this, lexer))
				{
					if (match.Current == null)
						continue;

					foreach (var match2 in GetMatches(match, rule, startElement + 1))
						yield return match2;
				}
			}
		}

		private static IGrammarRuleElement[] Compile(string rule)
		{
			if (rule == null) throw new ArgumentNullException(nameof(rule));

			var lexer = new Lexer("grammar_rule", rule.Replace('`', '"'));

			var conditions = new List<IGrammarRuleElement>();
			var nextConditionIsOptional = false;
			var add = new Action<IGrammarRuleElement>(element =>
			{
				conditions.Add(nextConditionIsOptional ? new GrammarRuleElementOptional(element) : element);
				nextConditionIsOptional = false;
			});

			var types =
				typeof (TokenType).GetEnumValues()
					.OfType<TokenType>()
					.Select(value => new KeyValuePair<string, TokenType>(value.ToUpperString(), value))
					.ToDictionary(p => p.Key, p => p.Value);

			while (lexer.MoveNext())
			{
				switch (lexer.Current.Type)
				{
					case TokenType.Token:
						add(new GrammarRuleElementLiteral(new[] {TokenType.Token}, lexer.Current.Value));
						break;
					case TokenType.Identifier:
						if (lexer.Current.Value == "OPTIONAL")
						{
							nextConditionIsOptional = true;
							break;
						}

						TokenType tokenType;
						if (types.TryGetValue(lexer.Current.Value, out tokenType))
						{
							var tokenTypes = new List<TokenType>(new[] {tokenType});
							for (;;)
							{
								var previous = lexer.Current;
								if (!lexer.MoveNext())
								{
									add(new GrammarRuleElementLiteral(tokenTypes.ToArray(), null));
									break;
								}

								if (!lexer.Current.Is(TokenType.Token, "|"))
								{
									lexer.Push(previous);
									add(new GrammarRuleElementLiteral(tokenTypes.ToArray(), null));
									break;
								}

								if (!lexer.MoveNext())
								{
									add(new GrammarRuleElementLiteral(tokenTypes.ToArray(), null));
									break;
								}

								lexer.AssertToken(TokenType.Identifier);
								if (types.TryGetValue(lexer.Current.Value, out tokenType))
									tokenTypes.Add(tokenType);
								else
									throw new Exception("Expected token type");
							}
							break;
						}

						add(new GrammarRuleElementEmbed(lexer.Current.Value));
						break;
					case TokenType.StringLiteral:
						add(new GrammarRuleElementLiteral(new[] {TokenType.Identifier}, lexer.Current.Value));
						break;
					default:
						throw new Exception("Invalid token");
				}
			}

			return conditions.ToArray();
		}
	}
}