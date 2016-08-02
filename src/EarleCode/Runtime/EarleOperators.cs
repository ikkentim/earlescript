﻿// EarleCode
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

namespace EarleCode.Runtime
{
    internal static class EarleOperators
    {
        private static Dictionary<string, OpCode> _binaryOperators = new Dictionary<string, OpCode>();
        private static Dictionary<string, OpCode> _unaryOperators = new Dictionary<string, OpCode>();
        private static Dictionary<OpCode, int> _binaryOperatorPriority = new Dictionary<OpCode, int>();
        private static List<OpCode> _assignmentOperators = new List<OpCode>();
        private static List<OpCode> _assignmentModOperators = new List<OpCode>();

        public static IEnumerable<string> BinaryOperators = _binaryOperators.Keys;
        public static IEnumerable<string> UnaryOperators = _unaryOperators.Keys;
        public static IEnumerable<string> AssignmentModOperators = _binaryOperators
            .Where(kv => _assignmentModOperators.Contains(kv.Value))
            .Select(kv => kv.Key + kv.Key);
        
        static EarleOperators()
        {
            // Build up dictionary of operators
            var ops = typeof(OpCode).GetEnumValues().OfType<OpCode>().ToArray();
 
            foreach(var op in ops)
            {
                var attribute = op.GetCustomAttribute<OperatorAttribute>();

                if(attribute == null)
                    continue;

                if(attribute.Type.HasFlag(OperatorType.UnaryOperator))
                    _unaryOperators.Add(attribute.Symbol, op);
                if(attribute.Type.HasFlag(OperatorType.BinaryOperator))
                {
                    _binaryOperators.Add(attribute.Symbol, op);
                    _binaryOperatorPriority.Add(op, attribute.Priority);

                    if(attribute.Type.HasFlag(OperatorType.AssignmentOperator))
                        _assignmentOperators.Add(op);
                    if(attribute.Type.HasFlag(OperatorType.AssignmentModOperator))
                        _assignmentModOperators.Add(op);
                }
            }
        }


        public static bool IsOperator(OperatorType type, string symbol)
        {
            if(symbol == null) throw new ArgumentNullException(nameof(symbol));

            switch(type)
            {
                case OperatorType.AssignmentOperator:
                    return IsOperator(OperatorType.BinaryOperator, symbol) &&
                        IsAssignmentModOperator(GetOpCode(OperatorType.BinaryOperator, symbol));
                case OperatorType.AssignmentModOperator:
                    {
                        var op = symbol.Substring(0, symbol.Length / 2);
                        return IsOperator(OperatorType.BinaryOperator, op) &&
                            IsAssignmentModOperator(GetOpCode(OperatorType.BinaryOperator, op));
                    }
                case OperatorType.UnaryOperator:
                    return _unaryOperators.ContainsKey(symbol);
                case OperatorType.BinaryOperator:
                    return _binaryOperators.ContainsKey(symbol);
                default:
                    return false;
            }
        }

        public static int GetPriority(OpCode opCode)
        {
            int result;
            return _binaryOperatorPriority.TryGetValue(opCode, out result) ? result : 0;
        }

        public static int GetMaxOperatorLength(OperatorType type, string startingWith)
        {
            if(startingWith == null) throw new ArgumentNullException(nameof(startingWith));

            switch(type)
            {
                case OperatorType.AssignmentModOperator:
                    {
                        var vals = AssignmentModOperators.Where(o => o.StartsWith(startingWith, StringComparison.InvariantCulture));
                        return vals.Any() ? vals.Max(o => o.Length) : 0;
                    }
                case OperatorType.AssignmentOperator:
                    {
                        var vals = BinaryOperators.Where(o => o.StartsWith(startingWith, StringComparison.InvariantCulture) && IsOperator(OperatorType.AssignmentOperator, o));
                        return vals.Any() ? vals.Max(o => o.Length) : 0;
                    }
                case OperatorType.UnaryOperator:
                    {
                        var vals = _unaryOperators.Keys.Where(o => o.StartsWith(startingWith, StringComparison.InvariantCulture));
                        return vals.Any() ? vals.Max(o => o.Length) : 0;
                    }
                case OperatorType.BinaryOperator:
                    {
                        var vals = _binaryOperators.Keys.Where(o => o.StartsWith(startingWith, StringComparison.InvariantCulture));
                        return vals.Any() ? vals.Max(o => o.Length) : 0;
                    }
                default:
                    return 0;
            }
        }

        public static OpCode GetOpCode(OperatorType type, string symbol)
        {
            if(symbol == null) throw new ArgumentNullException(nameof(symbol));

            switch(type)
            {
                case OperatorType.AssignmentModOperator:
                    if(!IsOperator(OperatorType.AssignmentModOperator, symbol))
                        throw new ArgumentException("Invalid symbol", nameof(symbol));
                    return GetOpCode(OperatorType.BinaryOperator, symbol.Substring(0, symbol.Length / 2));
                case OperatorType.UnaryOperator:
                    if(!_unaryOperators.ContainsKey(symbol))
                        throw new ArgumentException("Invalid symbol", nameof(symbol));
                    return _unaryOperators[symbol];
                case OperatorType.BinaryOperator:
                    if(!_binaryOperators.ContainsKey(symbol))
                        throw new ArgumentException("Invalid symbol", nameof(symbol));
                    return _binaryOperators[symbol];
                default:
                    throw new ArgumentException("Invalid symbol type", nameof(type));
            }
        }

        public static bool IsAssignmentOperator(OpCode opCode)
        {
            return _assignmentOperators.Contains(opCode);
        }

        public static bool IsAssignmentModOperator(OpCode opCode)
        {
            return _assignmentModOperators.Contains(opCode);
        }
    }
}