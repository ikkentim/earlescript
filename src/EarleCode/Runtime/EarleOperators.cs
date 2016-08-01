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
using EarleCode.Compiler.Lexing;
using EarleCode.Runtime.Instructions;

namespace EarleCode.Runtime
{
    internal static class EarleOperators
    {
        public struct OperatorInfo
        {
            public OperatorInfo(int priority, OpCode opCode)
            {
                Priority = priority;
                OpCode = opCode;
            }

            public OpCode OpCode { get; }
            public int Priority { get; }
        }

        public static readonly IDictionary<string, OperatorInfo> BinaryOperators = new Dictionary<string, OperatorInfo>
        {
            //TODO
            ["+"] = new OperatorInfo(1, OpCode.Add),//add
            ["-"] = new OperatorInfo(1, OpCode.Subtract),//sub
            ["*"] = new OperatorInfo(3, OpCode.Multiply),//mul
            ["/"] = new OperatorInfo(3, OpCode.Divide),//div
            ["^"] = new OperatorInfo(3, OpCode.BitwiseXor),//xor.bitwise
            ["|"] = new OperatorInfo(3, OpCode.BitwiseOr),//or.bitwise
            ["&"] = new OperatorInfo(3, OpCode.BitwiseAnd),//and.bitwise
            ["<<"] = new OperatorInfo(3, OpCode.Return),//shl
            [">>"] = new OperatorInfo(3, OpCode.Return),//shr
            ["<"] = new OperatorInfo(4, OpCode.CheckLessThan),//clt
            [">"] = new OperatorInfo(4, OpCode.CheckGreaterThan),//cgt
            ["<="] = new OperatorInfo(4, OpCode.CheckLessOrEqual),//clte
            [">="] = new OperatorInfo(4, OpCode.CheckGreaterOrEqual),//cgte
            ["=="] = new OperatorInfo(4, OpCode.CheckEqual),//ceq
            ["!="] = new OperatorInfo(4, OpCode.CheckNotEqual),//cneq
        };

        public static readonly IDictionary<string, OpCode> UnaryOperators = new Dictionary<string, OpCode>
        {
            //TODO
            ["-"] = OpCode.Negate,//neg
            ["!"] = OpCode.LogicalNot,//not.logical
            ["~"] = OpCode.BitwiseNot,//not.bitwise
            ["@"] = OpCode.Convert,//conv
        };

        public static readonly string[] UnaryAssignmentOperators =
        {
            "-"
        };

        public static readonly string[] UnaryAssignmentModOperators =
        {
            "++",
            "--"
        };

        public static readonly IDictionary<string, TokenType> UnaryOperatorTargets = new Dictionary<string, TokenType>
        {
            ["@"] = TokenType.StringLiteral
        };

        public static TokenType GetUnaryOperatorTarget(string unaryOperator)
        {
            if (unaryOperator == null) throw new ArgumentNullException(nameof(unaryOperator));
            TokenType target;
            return UnaryOperatorTargets.TryGetValue(unaryOperator, out target) ? target : TokenType.Identifier;
        }

        public static bool IsUnaryOpertorTargetValid(string unaryOperator, TokenType token)
        {
            if (unaryOperator == null) throw new ArgumentNullException(nameof(unaryOperator));
            TokenType target;
            return !UnaryOperatorTargets.TryGetValue(unaryOperator, out target) || target == token;
        }
    }
}