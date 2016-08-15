// EarleCode
// Copyright 2016 Tim Potze
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

namespace EarleCode.Runtime.Values
{
    /// <summary>
    ///     Provides functionality for getting and settings fields of the Earle structure.
    /// </summary>
    /// <seealso cref="IEarleObject" />
    public interface IEarleStructure : IEarleObject
    {
        /// <summary>
        ///     Gets the field with the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>The value of the field.</returns>
        EarleValue GetField(string name);

        /// <summary>
        ///     Sets the field with the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        void SetField(string name, EarleValue value);
    }
}