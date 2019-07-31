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

namespace EarleCode.Runtime.Attributes
{
    /// <summary>
    ///     Contains all <see cref="EarleAttributeScanner" /> scope options.
    /// </summary>
    public enum EarleAttributeScanScope
    {
        /// <summary>
        ///     Scan the assembly of the specified type.
        /// </summary>
        Assembly,

        /// <summary>
        ///     Scan only the specified class type.
        /// </summary>
        Class
    }
}