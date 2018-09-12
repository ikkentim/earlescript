﻿// EarleCode
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

namespace EarleCode.Compiling.Parsing.ParseTree
{
    /// <summary>
    ///     This interface is implemented by all parse tree nodes.
    /// </summary>
    public interface INode
    {
        /// <summary>
        /// Returns a string representing this node as a tree.
        /// </summary>
        /// <returns>A string representing this node as a tree.</returns>
	    string ToTreeString();
    }
}