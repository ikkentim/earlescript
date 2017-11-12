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
    ///     Contains flag values which can be attached to tokens.
    /// </summary>
    public enum TokenFlag : byte
    {
        /// <summary>
        ///     This token is emtpy.
        /// </summary>
        Empty,

        /// <summary>
        ///     Default flag.
        /// </summary>
        Default,

        /// <summary>
        ///     This token indicates the end of the file.
        /// </summary>
        EndOfFile
    }
}