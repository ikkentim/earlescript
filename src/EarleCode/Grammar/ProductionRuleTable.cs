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

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace EarleCode.Grammar
{
    internal class ProductionRuleTable : IEnumerable<ProductionRule>
    {
        private readonly Dictionary<string, List<ProductionRule>> _rules =
            new Dictionary<string, List<ProductionRule>>();

        [DebuggerHidden]
        public void Add(ProductionRule rule)
        {
            List<ProductionRule> r;
            if (!_rules.TryGetValue(rule.Name, out r))
                r = _rules[rule.Name] = new List<ProductionRule>();

            r.Add(rule);
        }

        [DebuggerHidden]
        public IEnumerable<ProductionRule> Get(string name)
        {
            List<ProductionRule> r;
            return _rules.TryGetValue(name, out r)
                ? (IEnumerable<ProductionRule>) r
                : new ProductionRule[0];
        }

        #region Implementation of IEnumerable

        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        ///     A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.
        /// </returns>
        [DebuggerHidden]
        public IEnumerator<ProductionRule> GetEnumerator()
        {
            return _rules.Values.SelectMany(v => v).GetEnumerator();
        }

        /// <summary>
        ///     Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        ///     An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        [DebuggerHidden]
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}