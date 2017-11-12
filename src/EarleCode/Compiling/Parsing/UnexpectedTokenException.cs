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

namespace EarleCode.Compiling.Parsing
{
    /// <summary>
    ///     Represents errors that occur during the parsing phase when an unexpected token was found.
    /// </summary>
    /// <typeparam name="TTokenType">The type of the token type.</typeparam>
    public class UnexpectedTokenException<TTokenType> : ParserException where TTokenType : struct, IConvertible
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="UnexpectedTokenException{TTokenType}" /> class.
        /// </summary>
        /// <param name="actual">The actual.</param>
        public UnexpectedTokenException(Token<TTokenType> actual) : this(null, actual)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="UnexpectedTokenException{TTokenType}" /> class.
        /// </summary>
        /// <param name="expectation">The expected token.</param>
        /// <param name="actual">The actual.</param>
        public UnexpectedTokenException(string expectation, Token<TTokenType> actual) :
            base(expectation == null
                ? $"{actual:FP} Unexpected token {actual:T v}."
                : $"{actual:FP} Unexpected token {actual:T v}; expected {expectation}.")
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="UnexpectedTokenException{TTokenType}" /> class.
        /// </summary>
        /// <param name="expectation">The expected token.</param>
        /// <param name="actual">The actual.</param>
        public UnexpectedTokenException(Token<TTokenType> expectation, Token<TTokenType> actual) :
            base($"{actual:FP} Unexpected {actual:T v}; expected {expectation:B}.")
        {
        }
    }
}