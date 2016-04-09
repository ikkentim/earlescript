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
using EarleCode.Grammar;
using EarleCode.Lexing;

namespace EarleCode
{
    public class GrammarProcessor
    {
        private readonly ProductionRuleTable _rules = new ProductionRuleTable();

        public string this[string key]
        {
            set { AddRule(key, value); }
        }

        public void AddRule(string key, string rule)
        {
            _rules.Add(Compile(key, rule.Replace('`', '"')));
        }

        public IEnumerable<string> GetMatches(ILexer lexer)
        {
            return
                _rules.Where(r => r.GetMatches(lexer, _rules).Any())
                    .Select(r => r.Name);
        }

        public string GetMatch(ILexer lexer)
        {
            return GetMatches(lexer).FirstOrDefault();
        }

        public bool Matches(ILexer lexer, string rule)
        {
            return _rules.Get(rule).Any(r => r.GetMatches(lexer, _rules).Any());
        }

        private ProductionRule Compile(string key, string rule)
        {
            if (rule == null) throw new ArgumentNullException(nameof(rule));

            var lexer = new Lexer("production_rule", rule);

            var conditions = new List<IRuleElement>();
            var nextConditionIsOptional = false;
            var add = new Action<IRuleElement>(element =>
            {
                element = nextConditionIsOptional ? new OptionalRuleElement(element) : element;
                nextConditionIsOptional = false;
                conditions.Add(element);
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
                        add(new LiteralRuleElement(new[] {TokenType.Token}, lexer.Current.Value));
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
                                    add(new LiteralRuleElement(tokenTypes.ToArray(), null));
                                    break;
                                }

                                if (!lexer.Current.Is(TokenType.Token, "|"))
                                {
                                    lexer.Push(previous);
                                    add(new LiteralRuleElement(tokenTypes.ToArray(), null));
                                    break;
                                }

                                if (!lexer.MoveNext())
                                {
                                    add(new LiteralRuleElement(tokenTypes.ToArray(), null));
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

                        add(new EmbedRuleElement(lexer.Current.Value));
                        break;
                    case TokenType.StringLiteral:
                        add(new LiteralRuleElement(new[] {TokenType.Identifier}, lexer.Current.Value));
                        break;
                    default:
                        throw new Exception("Invalid token");
                }
            }

            return new ProductionRule(key, conditions.ToArray());
        }
    }
}