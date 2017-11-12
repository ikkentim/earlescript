// EarleCode
// Copyright 2017 Tim Potze
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

namespace EarleCode.Compiling.Parsing.Productions
{
    /// <summary>
    ///     Represents a production rule set.
    /// </summary>
    /// <typeparam name="TTokenType">The type of the token type.</typeparam>
    public class ProductionRuleSet<TTokenType> : IProductionRuleSet<TTokenType> where TTokenType : struct, IConvertible
    {
        private readonly Dictionary<string, List<ProductionRule<TTokenType>>> _productionRules =
            new Dictionary<string, List<ProductionRule<TTokenType>>>();

        /// <summary>
        ///     Adds a production rule for the specified symbol.
        /// </summary>
        /// <param name="symbol">The symbol of the production rule to add.</param>
        /// <param name="rule">The production rule to add.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="symbol" /> or <paramref name="rule" /> is null.</exception>
        public void Add(string symbol, ProductionRule<TTokenType> rule)
        {
            if (symbol == null) throw new ArgumentNullException(nameof(symbol));
            if (rule == null) throw new ArgumentNullException(nameof(rule));

            if (_productionRules.TryGetValue(symbol, out var collection))
            {
                collection.Add(rule);
            }
            else
            {
                collection = new List<ProductionRule<TTokenType>> {rule};
                _productionRules[symbol] = collection;
            }
        }

        /// <summary>
        ///     Clears this set.
        /// </summary>
        public void Clear()
        {
            _productionRules.Clear();
        }

        #region Implementation of IProductionRuleSet<TTokenType>

        /// <summary>
        ///     Gets the default production symbol.
        /// </summary>
        public string Default { get; set; }

        /// <summary>
        ///     Gets a collection of all available production rules.
        /// </summary>
        public IEnumerable<ProductionRule<TTokenType>> All => _productionRules.Values.SelectMany(r => r);

        /// <summary>
        ///     Gets a collection of all available production rules which can be represented by the specified
        ///     <paramref name="symbol" />.
        /// </summary>
        /// <param name="symbol">The symbol of the production rules.</param>
        /// <returns>A collection of all available production rules which can be represented by the specified symbol.</returns>
        public IEnumerable<ProductionRule<TTokenType>> Get(string symbol)
        {
            _productionRules.TryGetValue(symbol, out var result);
            return result;
        }

        #endregion
    }
}