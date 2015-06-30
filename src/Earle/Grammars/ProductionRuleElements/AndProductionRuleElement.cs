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

using System;
using System.Collections.Generic;
using System.Linq;
using Earle.Grammars.ProductionRuleElements;
using Earle.Tokens;

namespace Earle.Grammars
{
    public class AndProductionRuleElement : IProductionRuleElement
    {
        private readonly List<IProductionRuleElement> _conditions = new List<IProductionRuleElement>();

        public AndProductionRuleElement(params IProductionRuleElement[] conditions)
        {
            if (conditions == null) throw new ArgumentNullException("conditions");
            _conditions.AddRange(conditions);
        }

        public IReadOnlyCollection<IProductionRuleElement> Conditions
        {
            get { return _conditions.AsReadOnly(); }
        }

        #region Implementation of IProductionRuleElement

        public bool Matches(TokenWalker tokenWalker, IEnumerable<ProductionRule> rules)
        {
            return Matches(tokenWalker, rules, 0);
        }

        #endregion

        public void AddCondition(IProductionRuleElement condition)
        {
            if (condition == null) throw new ArgumentNullException("condition");
            _conditions.Add(condition);
        }

        public bool Matches(TokenWalker tokenWalker, IEnumerable<ProductionRule> rules, int skip,
            ProductionRule except = null)
        {
            if (tokenWalker.Current == null)
                return false;

            tokenWalker.CreateSession();

            var result = true;

            for (var i = Math.Max(0, skip); i < _conditions.Count; i++)
            {
                var element = _conditions[i];
                if (!element.Matches(tokenWalker, i == 0 && except != null ? rules.Except(new[] {except}) : rules))
                {
                    result = false;
                    break;
                }

                if (i != _conditions.Count - 1 && tokenWalker.Current == null)
                {
                    result = false;
                    break;
                }
            }
            return
                tokenWalker.FlushOrDropSession(result);
        }

        #region Overrides of Object

        public override string ToString()
        {
            return string.Join(" ", (object[]) _conditions.ToArray());
        }

        #endregion
    }
}