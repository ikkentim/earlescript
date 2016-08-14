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
    internal static class EarleOperators
    {
        private static readonly Dictionary<string, OpCode> BinaryOpCodes = new Dictionary<string, OpCode>();
        private static readonly Dictionary<string, OpCode> UnaryOpCodes = new Dictionary<string, OpCode>();
        private static readonly Dictionary<OpCode, int> BinaryPriorities = new Dictionary<OpCode, int>();

        private static readonly List<OpCode> AssignmentOpCodes = new List<OpCode>();
        private static readonly List<OpCode> AssignmentModOpCodes = new List<OpCode>();

        public static readonly IEnumerable<string> BinaryOperators;
        public static readonly IEnumerable<string> UnaryOperators;
        public static readonly IEnumerable<string> AssignmentModOperators;
        public static readonly IEnumerable<string> AssignmentOperators;


        static EarleOperators()
        {
            // Build up dictionary of operators
            var ops = typeof (OpCode).GetEnumValues().OfType<OpCode>().ToArray();

            foreach (var op in ops)
            {
                var attribute = op.GetCustomAttribute<OperatorAttribute>();

                if (attribute == null)
                    continue;

                if (attribute.Type.HasFlag(EarleOperatorType.UnaryOperator))
                    UnaryOpCodes.Add(attribute.Symbol, op);
                if (attribute.Type.HasFlag(EarleOperatorType.BinaryOperator))
                {
                    BinaryOpCodes.Add(attribute.Symbol, op);
                    BinaryPriorities.Add(op, attribute.Priority);

                    if (attribute.Type.HasFlag(EarleOperatorType.AssignmentOperator))
                        AssignmentOpCodes.Add(op);
                    if (attribute.Type.HasFlag(EarleOperatorType.AssignmentModOperator))
                        AssignmentModOpCodes.Add(op);
                }
            }

            BinaryOperators = BinaryOpCodes.Keys.ToArray();
            UnaryOperators = UnaryOpCodes.Keys.ToArray();
            AssignmentModOperators = BinaryOpCodes
                .Where(kv => AssignmentModOpCodes.Contains(kv.Value))
                .Select(kv => kv.Key + kv.Key)
                .ToArray();
            AssignmentOperators = BinaryOpCodes
                .Where(kv => AssignmentOpCodes.Contains(kv.Value))
                .Select(kv => kv.Key)
                .ToArray();
        }

        private static IEnumerable<string> GetOperatorsOfType(EarleOperatorType type)
        {
            switch (type)
            {
                case EarleOperatorType.AssignmentModOperator:
                    return AssignmentModOperators;
                case EarleOperatorType.AssignmentOperator:
                    return AssignmentOperators;
                case EarleOperatorType.BinaryOperator:
                    return BinaryOperators;
                case EarleOperatorType.UnaryOperator:
                    return UnaryOperators;
                default:
                    throw new ArgumentException("Invalid symbol type", nameof(type));
            }
        }

        public static bool IsOperator(EarleOperatorType type, string symbol)
        {
            if (symbol == null) throw new ArgumentNullException(nameof(symbol));

            return GetOperatorsOfType(type).Contains(symbol);
        }

        public static int GetPriority(OpCode opCode)
        {
            int result;
            return BinaryPriorities.TryGetValue(opCode, out result) ? result : 0;
        }

        public static int GetMaxOperatorLength(EarleOperatorType type, string startingWith)
        {
            if (startingWith == null) throw new ArgumentNullException(nameof(startingWith));

            var vals = GetOperatorsOfType(type)
                .Where(o => o.StartsWith(startingWith, StringComparison.InvariantCulture));
            return vals.Any() ? vals.Max(o => o.Length) : 0;
        }

        public static OpCode GetOpCode(EarleOperatorType type, string symbol)
        {
            if (symbol == null) throw new ArgumentNullException(nameof(symbol));

            if (!IsOperator(type, symbol))
                throw new ArgumentException("Invalid symbol", nameof(symbol));

            switch (type)
            {
                case EarleOperatorType.AssignmentModOperator:
                    return GetOpCode(EarleOperatorType.BinaryOperator, symbol.Substring(0, symbol.Length/2));
                case EarleOperatorType.AssignmentOperator:
                    return GetOpCode(EarleOperatorType.BinaryOperator, symbol);
                case EarleOperatorType.UnaryOperator:
                    return UnaryOpCodes[symbol];
                case EarleOperatorType.BinaryOperator:
                    return BinaryOpCodes[symbol];
                default:
                    throw new ArgumentException("Invalid symbol type", nameof(type));
            }
        }

        public static bool IsAssignmentOperator(OpCode opCode)
        {
            return AssignmentOpCodes.Contains(opCode);
        }

        public static bool IsAssignmentModOperator(OpCode opCode)
        {
            return AssignmentModOpCodes.Contains(opCode);
        }
    }
}