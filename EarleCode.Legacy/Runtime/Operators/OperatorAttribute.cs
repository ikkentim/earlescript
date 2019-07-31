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
using EarleCode.Runtime.Instructions;

namespace EarleCode.Runtime.Operators
{
    /// <summary>
    ///     Holds information about an operator <see cref="OpCode" />
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Field)]
    internal class OperatorAttribute : Attribute
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="OperatorAttribute" /> class.
        /// </summary>
        /// <param name="symbol">The symbol.</param>
        /// <param name="type">The type.</param>
        /// <param name="priority">The priority.</param>
        public OperatorAttribute(string symbol, EarleOperatorType type, int priority = 0)
        {
            Symbol = symbol;
            Type = type;
            Priority = priority;
        }

        /// <summary>
        ///     Gets the symbol.
        /// </summary>
        public string Symbol { get; }

        /// <summary>
        ///     Gets the type.
        /// </summary>
        public EarleOperatorType Type { get; }

        /// <summary>
        ///     Gets the priority.
        /// </summary>
        public int Priority { get; }
    }
}