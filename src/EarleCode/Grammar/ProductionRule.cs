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
using EarleCode.Lexing;

namespace EarleCode.Grammar
{
    internal class ProductionRule
    {
        public ProductionRule(string name, IRuleElement[] conditions)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (conditions == null) throw new ArgumentNullException(nameof(conditions));
            Name = name;
            Conditions = conditions;
        }

        public string Name { get; }

        public IRuleElement[] Conditions { get; }

        public IEnumerable<ILexer> GetMatches(ILexer lexer, ProductionRuleTable rules, int skip = 0)
        {
            return Conditions.Skip(skip)
                .Aggregate((IEnumerable<ILexer>) new[] {lexer},
                    (current, condition) => current.SelectMany(l => condition.GetMatches(l, rules)));
        }

        #region Overrides of Object

        /// <summary>
        ///     Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        ///     A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            return $"{Name} = {string.Join(" ", (object[]) Conditions)}";
        }

        #endregion
    }
}