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

namespace EarleCode.Runtime.Operators
{
    public enum EarleOperatorParamOrder
    {
        /// <summary>
        /// The order specified by the definition.
        /// </summary>
        Normal,
        /// <summary>
        /// The order specified by the code.
        /// </summary>
        Any,
        /// <summary>
        /// The order specified by the definition. This may mean the left and right values are swapped.
        /// </summary>
        Specified,
        /// <summary>
        /// The reverse order as specified by the code.
        /// </summary>
        Swap
    }
}