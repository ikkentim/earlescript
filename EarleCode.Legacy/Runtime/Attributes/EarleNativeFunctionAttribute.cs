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

using System;

namespace EarleCode.Runtime.Attributes
{
    /// <summary>
    ///     Indicates the method is an native Earle function.
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Method)]
    public class EarleNativeFunctionAttribute : Attribute
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="EarleNativeFunctionAttribute" /> class.
        /// </summary>
        public EarleNativeFunctionAttribute()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="EarleNativeFunctionAttribute" /> class.
        /// </summary>
        /// <param name="name">The name of the native function.</param>
        public EarleNativeFunctionAttribute(string name)
        {
            Name = name;
        }

        /// <summary>
        ///     Gets the name of the native function.
        /// </summary>
        public string Name { get; }
    }
}