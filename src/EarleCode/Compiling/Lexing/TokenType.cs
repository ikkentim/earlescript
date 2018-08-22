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

namespace EarleCode.Compiling.Lexing
{
    /// <summary>
    ///     Contains the different token types which can be found in an Earle script.
    /// </summary>
    public enum TokenType : byte
    {
        /// <summary>
        ///     A symbol. A symbol is any character which does not match any other token type.
        /// </summary>
        Symbol,

        /// <summary>
        ///     An identifier. An identifier starts with any alphabetic or '_' character, followed by any number of alphanumeric or
        ///     '_' characters.
        /// </summary>
        Identifier,

        /// <summary>
        ///     A number literal. Can be any integer or decimal number.
        /// </summary>
        NumberLiteral,

        /// <summary>
        ///     A string literal. Starts and ends with a double quote ("). Any uses of double quotes in the string literal can be
        ///     escaped.
        /// </summary>
        StringLiteral,
        
        /// <summary>
        ///     A keyword.
        /// </summary>
        Keyword
    }
}