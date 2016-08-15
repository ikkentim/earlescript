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
using System.Collections.Generic;
using System.Linq;
using EarleCode.Runtime.Instructions;
using EarleCode.Utilities;

namespace EarleCode.Runtime.Operators
{
    /// <summary>
    ///     Utility containing all supported Earle operators.
    /// </summary>
    internal static class EarleOperators
    {
        private static readonly Dictionary<OpCode, int> Priorities = new Dictionary<OpCode, int>();

        private static readonly Dictionary<EarleOperatorType, Dictionary<string, OpCode>> Operators =
            new Dictionary<EarleOperatorType, Dictionary<string, OpCode>>();

        /// <summary>
        ///     Initializes the <see cref="EarleOperators" /> class.
        /// </summary>
        static EarleOperators()
        {
            // Build up dictionary of operators
            foreach (var op in typeof(OpCode).GetEnumValues().OfType<OpCode>().ToArray())
            {
                var attribute = op.GetCustomAttribute<OperatorAttribute>();

                if (attribute == null)
                    continue;

                foreach (var flag in typeof (EarleOperatorType).GetEnumValues().OfType<EarleOperatorType>())
                    if (attribute.Type.HasFlag(flag))
                        GetOperators(flag).Add(flag == EarleOperatorType.AssignmentModOperator
                            ? attribute.Symbol + attribute.Symbol
                            : attribute.Symbol, op);


                Priorities.Add(op, attribute.Priority);
            }
        }

        /// <summary>
        ///     Gets the operators of the specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The operators of the specified type.</returns>
        public static Dictionary<string, OpCode> GetOperators(EarleOperatorType type)
        {
            Dictionary<string, OpCode> value;
            if (!Operators.TryGetValue(type, out value))
                Operators[type] = value = new Dictionary<string, OpCode>();

            return value;
        }

        /// <summary>
        ///     Gets the operator symbols of the specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The operator symbols of the specified type.</returns>
        public static IEnumerable<string> GetOperatorSymbols(EarleOperatorType type)
        {
            return GetOperators(type).Keys;
        }

        /// <summary>
        ///     Gets the operator with the specified type and symbol
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="symbol">The symbol.</param>
        /// <returns>The operation code of the operator.</returns>
        public static OpCode GetOperator(EarleOperatorType type, string symbol)
        {
            OpCode value;
            GetOperators(type).TryGetValue(symbol, out value);
            return value;
        }

        /// <summary>
        ///     Gets the priority of the specified operation code's operator.
        /// </summary>
        /// <param name="opCode">The operation code code.</param>
        /// <returns>The priority of the speicfied operation code's operator.</returns>
        public static int GetPriority(OpCode opCode)
        {
            int result;
            return Priorities.TryGetValue(opCode, out result) ? result : 0;
        }

        /// <summary>
        ///     Gets the maximum length of operator symbols starting with <see cref="startingWith" />.
        /// </summary>
        /// <param name="type">The type of the operators.</param>
        /// <param name="startingWith">The starting with.</param>
        /// <returns>The maximum length</returns>
        /// <exception cref="System.ArgumentNullException">Thrown of <see cref="startingWith" /> is null.</exception>
        public static int GetMaxOperatorLength(EarleOperatorType type, string startingWith)
        {
            if (startingWith == null) throw new ArgumentNullException(nameof(startingWith));

            var vals = GetOperatorSymbols(type)
                .Where(o => o.StartsWith(startingWith, StringComparison.InvariantCulture))
                .ToArray();
            return vals.Any() ? vals.Max(o => o.Length) : 0;
        }
    }
}