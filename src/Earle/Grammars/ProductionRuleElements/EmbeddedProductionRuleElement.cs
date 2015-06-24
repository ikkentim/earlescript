// Earle
// Copyright 2015 Tim Potze
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
using Earle.Grammars.ProductionRuleElements;
using Earle.Tokens;

namespace Earle.Grammars
{
    public class EmbeddedProductionRuleElement : IProductionRuleElement
    {
        public EmbeddedProductionRuleElement(string rule)
        {
            Rule = rule;
        }

        public string Rule { get; private set; }

        #region Implementation of IProductionRuleElement

        public bool Matches(TokenWalker tokenWalker, ICollection<ProductionRule> rules)
        {
            if (tokenWalker.Current == null)
                return false;

            return rules.Where(rule => rule.Name == Rule)
                .Where(rule => !IsRecursiveProductionRule(rule.Rule.Conditions.First())).Any(rule =>
                {
                    if (rule.Rule.Matches(tokenWalker, rules))
                    {
                        rules.Where(rule2 => rule2.Name == Rule)
                            .Where(rule2 => IsRecursiveProductionRule(rule2.Rule.Conditions.First()))
                            // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
                            .Any(rule2 => rule2.Rule.Matches(tokenWalker, rules, 1));
                        return true;
                    }
                    return false;
                });
        }

        #endregion

        private bool IsRecursiveProductionRule(IProductionRuleElement element)
        {
            return element is EmbeddedProductionRuleElement &&
                   ((EmbeddedProductionRuleElement) element).Rule == Rule;
        }

        #region Overrides of Object

        public override string ToString()
        {
            return Rule;
        }

        #endregion
    }
}