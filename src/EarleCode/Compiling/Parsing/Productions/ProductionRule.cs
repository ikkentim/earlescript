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
using System.Linq;

namespace EarleCode.Compiling.Parsing.Productions
{
    /// <summary>
    ///     Represents a single production rule.
    /// </summary>
    /// <typeparam name="TTokenType">The type of the token type.</typeparam>
    public class ProductionRule<TTokenType> where TTokenType : struct, IConvertible
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ProductionRule{TTokenType}" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="elements">The elements.</param>
        public ProductionRule(string name, ProductionRuleElement<TTokenType>[] elements)
        {
            Name = name;
            Elements = elements;
        }

        /// <summary>
        ///     Gets or sets the symbol name of this production rule.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Gets or sets the elements of this production rule.
        /// </summary>
        public ProductionRuleElement<TTokenType>[] Elements { get; set; }

        #region Overrides of Object

        /// <summary>
        ///     Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        ///     A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Name + " -> " + string.Join(" ", Elements.Select(n => n.ToString()));
        }

        #endregion
    }
}