// EarleCode
// Copyright 2015 Tim Potze
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

namespace EarleCode.Functions
{
    public struct EarleFunctionSignature
    {
        public EarleFunctionSignature(string name, string path)
        {
            Name = name;
            Path = path;
        }

        public string Name { get; }
        public string Path { get; }

        #region Overrides of ValueType

        /// <summary>
        /// Returns the fully qualified type name of this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> containing a fully qualified type name.
        /// </returns>
        public override string ToString()
        {
            return $"{Path}::{Name}";
        }

        #endregion
    }
}