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
using EarleCode.Lexing;

namespace EarleCode.Grammar.RulesElements
{
    public class OptionalGrammarRuleElement : IGrammarRuleElement
    {
        private readonly IGrammarRuleElement _element;

        public OptionalGrammarRuleElement(IGrammarRuleElement element)
        {
            if (element == null) throw new ArgumentNullException(nameof(element));
            _element = element;
        }

        #region Implementation of IGrammarRuleElement

        public ProductionRuleMatchResult Matches(TokenWalker tokenWalker, IEnumerable<GrammarRule> rules)
        {
            if (tokenWalker.Current == null)
                return ProductionRuleMatchResult.True;

            tokenWalker.CreateSession();

            return tokenWalker.FlushOrDropSession(_element.Matches(tokenWalker, rules) == ProductionRuleMatchResult.True)
                ? ProductionRuleMatchResult.Optional
                : ProductionRuleMatchResult.True;
        }

        #endregion

        #region Overrides of Object

        public override string ToString()
        {
            return $"OPTIONAL {_element}";
        }

        #endregion
    }
}