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
using EarleCode.Lexing;

namespace EarleCode
{
    internal static class EarleOperators
    {
        public static readonly IDictionary<string, int> BinaryOperators = new Dictionary<string, int>
        {
            ["+"] = 1,
            ["-"] = 1,
            ["*"] = 3,
            ["/"] = 3,
            ["^"] = 3,
            ["|"] = 3,
            ["&"] = 3,
            ["<<"] = 3,
            [">>"] = 3,
            ["<"] = 4,
            [">"] = 4,
            ["<="] = 4,
            [">="] = 4,
            ["=="] = 4,
            ["!="] = 4,
            ["||"] = 5,
            ["&&"] = 5
        };

        public static readonly string[] UnaryOperators =
        {
            "+",
            "-",
            "!",
            "~",
            "@",
        };

        public static readonly string[] UnaryAssignmentOperators =
        {
            "+",
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