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
using EarleCode.Compiling.Parsing.Grammars.Productions;

namespace EarleCode.Compiling.Parsing.Grammars
{
    /// <summary>
    ///     Contains the methods of a production rules set.
    /// </summary>
    public interface IGrammar
    {
        /// <summary>
        ///     Gets the default production symbol.
        /// </summary>
        string Default { get; }

        /// <summary>
        ///     Gets a collection of all available production rules.
        /// </summary>
        IEnumerable<ProductionRule> All { get; }

	    /// <summary>
	    ///		Gets a collection of all non-terminal symbols for which production rules have been defined by this grammar.
	    /// </summary>
	    IEnumerable<string> NonTerminals { get; }

	    /// <summary>
	    ///		Gets a collection of all terminal symbols.
	    /// </summary>
	    IEnumerable<Terminal> Terminals { get; }

		/// <summary>
		///     Gets a collection of all available production rules which can be represented by the specified
		///     <paramref name="symbol" />.
		/// </summary>
		/// <param name="symbol">The symbol of the production rules.</param>
		/// <returns>A collection of all available production rules which can be represented by the specified symbol.</returns>
		IEnumerable<ProductionRule> Get(string symbol);
    }
}