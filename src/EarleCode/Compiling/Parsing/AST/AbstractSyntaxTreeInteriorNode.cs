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

using System.Collections.Generic;

namespace EarleCode.Compiling.Parsing.AST
{
    /// <summary>
    ///     Reprents an interior AST node.
    /// </summary>
    public class AbstractSyntaxTreeInteriorNode : IAbstractSyntaxTreeInteriorNode
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="AbstractSyntaxTreeInteriorNode" /> class.
        /// </summary>
        /// <param name="rule">The production rule symbol the node represents.</param>
        /// <param name="children">The children of the node.</param>
        public AbstractSyntaxTreeInteriorNode(string rule, IEnumerable<IAbstractSyntaxTreeNode> children)
        {
            Rule = rule;
            Children = children;
        }

        #region Implementation of IAbstractSyntaxTreeInteriorNode

        /// <summary>
        ///     Gets the production rule symbol this node represents.
        /// </summary>
        public string Rule { get; }

        /// <summary>
        ///     Gets a collection of child nodes of this node.
        /// </summary>
        public IEnumerable<IAbstractSyntaxTreeNode> Children { get; }

        #endregion

        public override string ToString()
        {
            return string.Join(" ", Children);
        }
    }
}