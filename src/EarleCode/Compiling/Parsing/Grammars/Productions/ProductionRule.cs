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

using System.Linq;

namespace EarleCode.Compiling.Parsing.Grammars.Productions
{
    /// <summary>
    ///     Represents a single production rule.
    /// </summary>
    public class ProductionRule
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ProductionRule" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="elements">The elements.</param>
        public ProductionRule(string name, ProductionRuleElement[] elements)
        {
            Name = name;
            Elements = elements;
        }

        /// <summary>
        ///     Gets or sets the symbol name of this production rule.
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     Gets or sets the elements of this production rule.
        /// </summary>
        public ProductionRuleElement[] Elements { get; }
    
        /// <summary>
        /// Gets a value indicating whether the rule contains epsilon.
        /// </summary>
        public bool IsEpsilon => Elements.Length == 0;
        
        #region Overrides of Object

        /// <summary>
        ///     Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        ///     A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return IsEpsilon
                ? $"{Name} -> \x03B5"
                : $"{Name} -> {string.Join(" ", Elements.Select(n => n.ToString()))}";
        }

        #endregion
    }
}