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
using EarleCode.Lexing;

namespace EarleCode.Grammar.RulesElements
{
    public class AndGrammarRuleElement : IGrammarRuleElement
    {
        private readonly List<IGrammarRuleElement> _conditions = new List<IGrammarRuleElement>();

        public AndGrammarRuleElement(params IGrammarRuleElement[] conditions)
        {
            if (conditions == null) throw new ArgumentNullException(nameof(conditions));
            _conditions.AddRange(conditions);
        }

        public IReadOnlyCollection<IGrammarRuleElement> Conditions => _conditions.AsReadOnly();

        #region Implementation of IGrammarRuleElement

        public ProductionRuleMatchResult Matches(TokenWalker tokenWalker, IEnumerable<GrammarRule> rules)
        {
            return Matches(tokenWalker, rules, 0) ? ProductionRuleMatchResult.True : ProductionRuleMatchResult.False;
        }
        #endregion
        
        public void AddCondition(IGrammarRuleElement condition)
        {
            if (condition == null) throw new ArgumentNullException(nameof(condition));
            _conditions.Add(condition);
        }

        public bool Matches(TokenWalker tokenWalker, IEnumerable<GrammarRule> rules, int skip,
            GrammarRule except = null)
        {
            if (tokenWalker.Current == null)
                return false;
            
            var result = true;
            
            var optional = new Stack<int>();
            var sessions = 0;

            for (var i = Math.Max(0, skip); i < _conditions.Count; i++)
            {
                tokenWalker.CreateSession();
                sessions++;

                var element = _conditions[i];

                var r = element.Matches(tokenWalker, i == 0 && except != null ? rules.Except(new[] {except}) : rules);

                if (r == ProductionRuleMatchResult.False)
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

                if (r == ProductionRuleMatchResult.Optional)
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
        }

        #region Overrides of Object

        public override string ToString()
        {
            return string.Join(" ", (object[]) _conditions.ToArray());
        }

        #endregion
    }
}