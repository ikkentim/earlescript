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
using EarleCode.OldCompiler.Lexing;

namespace EarleCode.OldCompiler.Grammar
{
	internal class GrammarRuleElementEmbed : IGrammarRuleElement
	{
		public GrammarRuleElementEmbed(string ruleName)
		{
			if (ruleName == null) throw new ArgumentNullException(nameof(ruleName));

			RuleName = ruleName;
		}

		public string RuleName { get; }

		public IEnumerable<ILexer> Match(GrammarProcessor grammar, ILexer lexer)
		{
			return grammar.GetMatches(lexer, RuleName);
		}

		public override string ToString()
		{
			return RuleName;
		}
	}
}