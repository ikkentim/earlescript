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
using EarleCode.Compiling.Parsing.ParseTree;

namespace EarleCode.Compiling.Parsing
{
    /// <summary>
    ///     Contains the methods of a parser.
    /// </summary>
    public interface IParser
    {
        /// <summary>
        ///     Parses the specified tokens into a parse tree.
        /// </summary>
        /// <param name="tokens">The tokens to parse.</param>
        /// <returns>The parse tree.</returns>
        /// <exception cref="ParserException">Thrown if an error occurs while parsing the specified tokens.</exception>
        INode Parse(Token[] tokens);
    }
}