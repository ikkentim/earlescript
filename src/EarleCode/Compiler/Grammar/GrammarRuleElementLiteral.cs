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
using System.Diagnostics;
using System.Linq;
using EarleCode.Compiler.Lexing;

namespace EarleCode.Compiler.Grammar
{
	internal class GrammarRuleElementLiteral : IGrammarRuleElement
	{
		public GrammarRuleElementLiteral(TokenType[] tokenTypes, string tokenValue)
		{
			if (tokenTypes == null) throw new ArgumentNullException(nameof(tokenTypes));
			TokenTypes = tokenTypes;
			TokenValue = tokenValue;
		}

		private TokenType[] TokenTypes { get; }

		private string TokenValue { get; }

		[DebuggerHidden]
		public IEnumerable<ILexer> Match(GrammarProcessor grammar, ILexer lexer)
		{
			foreach (var tokenType in TokenTypes)
			{
				if (TokenValue == null ? lexer.Current.Is(tokenType) : lexer.Current.Is(tokenType, TokenValue))
				{
					var newLexer = (ILexer) lexer.Clone();
					newLexer.MoveNext();
					yield return newLexer;
				}
			}
		}

		public override string ToString()
		{
			if (TokenValue == null)
				return string.Join("|", TokenTypes.Select(t => t.ToUpperString()));
			return TokenTypes.Length == 1 && TokenTypes[0] == TokenType.Token ? TokenValue : $"`{TokenValue}`";
		}
	}
}