// EarleCode
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
using EarleCode.Tokens;

namespace EarleCode.Grammar.RulesElements
{
    public class AndProductionRuleElement : IProductionRuleElement
    {
        private readonly List<IProductionRuleElement> _conditions = new List<IProductionRuleElement>();

        public AndProductionRuleElement(params IProductionRuleElement[] conditions)
        {
            if (conditions == null) throw new ArgumentNullException(nameof(conditions));
            _conditions.AddRange(conditions);
        }

        public IReadOnlyCollection<IProductionRuleElement> Conditions => _conditions.AsReadOnly();

        #region Implementation of IProductionRuleElement

        public PruductionRuleMatchResult Matches(TokenWalker tokenWalker, IEnumerable<ProductionRule> rules)
        {
            return Matches(tokenWalker, rules, 0) ? PruductionRuleMatchResult.True : PruductionRuleMatchResult.False;
        }
        #endregion
        
        public void AddCondition(IProductionRuleElement condition)
        {
            if (condition == null) throw new ArgumentNullException(nameof(condition));
            _conditions.Add(condition);
        }

        public bool Matches(TokenWalker tokenWalker, IEnumerable<ProductionRule> rules, int skip,
            ProductionRule except = null)
        {
            if (tokenWalker.Current == null)
                return false;
            
            //tokenWalker.CreateSession();

            var result = true;
            
            Stack<int> optional = new Stack<int>();
            int sessions = 0;

            for (var i = Math.Max(0, skip); i < _conditions.Count; i++)
            {
                tokenWalker.CreateSession();
                sessions++;

                var element = _conditions[i];

                var r = element.Matches(tokenWalker, i == 0 && except != null ? rules.Except(new[] {except}) : rules);

                if (r == PruductionRuleMatchResult.False)
                {
                    if (!optional.Any())
                    {
                        result = false;
                        break;
                    }

                    var oi = optional.Pop();

                    for (var j = 0; j < i - oi + 1; j++)
                    {
                        tokenWalker.DropSession();
                        sessions--;
                    }
                    i = oi;
                    continue;
                }

                if (r == PruductionRuleMatchResult.Optional)
                    optional.Push(i);

                if (i != _conditions.Count - 1 && tokenWalker.Current == null)
                {
                    if (!optional.Any())
                    {
                        result = false;
                        break;
                    }

                    var oi = optional.Pop();

                    if (i == oi)
                    {
                        tokenWalker.DropSession();
                        sessions--;
                        continue;
                    }

                    for (var j = 0; j < i - oi + 1; j++)
                    {
                        tokenWalker.DropSession();
                        sessions--;
                    }
                    i = oi;
                    continue;
                }
            }

            for (var j = 0; j < sessions; j++)
            {
                tokenWalker.FlushOrDropSession(result);
            }

            return result;
            //return tokenWalker.FlushOrDropSession(result);
        }

        #region Overrides of Object

        public override string ToString()
        {
            return string.Join(" ", (object[]) _conditions.ToArray());
        }

        #endregion
    }
}