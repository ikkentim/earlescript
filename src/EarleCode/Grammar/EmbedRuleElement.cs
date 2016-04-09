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

using System.Collections.Generic;
using System.Linq;
using EarleCode.Lexing;

namespace EarleCode.Grammar
{
    internal class EmbedRuleElement : IRuleElement
    {
        public EmbedRuleElement(string rule)
        {
            Rule = rule;
        }

        public string Rule { get; }

        #region Implementation of IRuleElement

        public IEnumerable<ILexer> GetMatches(ILexer lexer, ProductionRuleTable rules)
        {
            var result = new List<ILexer>();
            var laters = new List<ProductionRule>();
            foreach (var rule in rules.Get(Rule))
            {
                if ((rule.Conditions.First() as EmbedRuleElement)?.Rule == Rule)
                {
                    laters.Add(rule);
                }
                else
                    result.AddRange(rule.GetMatches(lexer, rules));
            }

            if (!laters.Any())
                return result;

            var laterSource = result;
            var laterResult = new List<ILexer>();

            for (;;)
            {
                foreach (var later in laters)
                    foreach (var l in laterSource)
                        laterResult.AddRange(later.GetMatches(l, rules, 1));

                if (laterResult.Any())
                {
                    laterSource = new List<ILexer>(laterResult);
                    laterResult.Clear();
                }
                else
                {
                    break;
                }
            }

            return result.Concat(laterSource);
        }

        #endregion

        #region Overrides of Object

        /// <summary>
        ///     Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        ///     A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            return Rule;
        }

        #endregion
    }
}