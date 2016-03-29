﻿// EarleCode
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

namespace EarleCode.Grammar.RulesElements
{
    public class EmbeddedGrammarRuleElement : IGrammarRuleElement
    {
        public EmbeddedGrammarRuleElement(string rule)
        {
            Rule = rule;
        }

        public string Rule { get; }

        #region Implementation of IGrammarRuleElement

        public ProductionRuleMatchResult Matches(TokenWalker tokenWalker, IEnumerable<GrammarRule> rules)
        {
            if (tokenWalker.Current == null)
                return ProductionRuleMatchResult.False;

            var productionRules = rules as GrammarRule[] ?? rules.ToArray();
            return productionRules.Where(rule => rule.Name == Rule)
                .Where(rule => !IsRecursiveProductionRule(rule.Rule.Conditions.First())).Any(rule =>
                {
                    tokenWalker.CreateSession();
                    if (rule.Rule.Matches(tokenWalker, productionRules) == ProductionRuleMatchResult.True)
                    {
                        foreach (var rule2 in productionRules)
                            if (rule2.Name == Rule && IsRecursiveProductionRule(rule2.Rule.Conditions.First()) &&
                                rule2.Rule.Matches(tokenWalker, productionRules, 1))
                                break;

                        tokenWalker.FlushSession();
                        return true;
                    }
                    tokenWalker.DropSession();
                    return false;
                })
                ? ProductionRuleMatchResult.True
                : ProductionRuleMatchResult.False;
        }

        #endregion

        private bool IsRecursiveProductionRule(IGrammarRuleElement element)
        {
            return element is EmbeddedGrammarRuleElement &&
                   ((EmbeddedGrammarRuleElement) element).Rule == Rule;
        }

        #region Overrides of Object

        public override string ToString()
        {
            return Rule;
        }

        #endregion
    }
}