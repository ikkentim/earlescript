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
using EarleCode.Compiling.Lexing;

namespace EarleCode.Compiling.Parsing.AST
{
    /// <summary>
    ///     Represents a leaf AST node.
    /// </summary>
    /// <typeparam name="TTokenType">The type enum of the token type inside this node.</typeparam>
    public class AbstractSyntaxTreeLeafNode<TTokenType> : IAbstractSyntaxTreeLeafNode<TTokenType>
        where TTokenType : struct, IConvertible
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="AbstractSyntaxTreeLeafNode{TTokenType}" /> class.
        /// </summary>
        /// <param name="token">The token.</param>
        public AbstractSyntaxTreeLeafNode(Token<TTokenType> token)
        {
            Token = token;
        }

        #region Implementation of IAbstractSyntaxTreeLeafNode<TTokenType>

        /// <summary>
        ///     Gets the token inside this node.
        /// </summary>
        public Token<TTokenType> Token { get; }

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