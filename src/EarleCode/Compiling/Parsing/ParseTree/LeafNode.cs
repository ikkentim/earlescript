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

using EarleCode.Compiling.Lexing;

namespace EarleCode.Compiling.Parsing.ParseTree
{
    /// <summary>
    ///     Represents a leaf syntax parse node.
    /// </summary>
    public class LeafNode : ILeafNode
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="LeafNode" /> class.
        /// </summary>
        /// <param name="token">The token.</param>
        public LeafNode(Token token)
        {
            Token = token;
        }

        #region Implementation of IAbstractSyntaxTreeLeafNode

        /// <summary>
        ///     Gets the token inside this node.
        /// </summary>
        public Token Token { get; }

        #endregion

	    #region Implementation of INode

        /// <summary>
        /// Returns a string representing this node as a tree.
        /// </summary>
        /// <returns>A string representing this node as a tree.</returns>
	    public string ToTreeString()
	    {
		    return Token.ToString("s b");
	    }

	    #endregion

        /// <summary>
        ///     Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        ///     A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Token.Value;
        }

    }
}