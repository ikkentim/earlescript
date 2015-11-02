// EarleCode
// Copyright 2015 Tim Potze
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

namespace EarleCode.Operators
{
    public static class OperatorUtil
    {
        public static int GetOperatorIdentifier(string operatorToken)
        {
            switch (operatorToken)
            {
                case "+":
                    return 0;
                case "-":
                    return 1;
                case "^":
                    return 2;
                case "&":
                    return 3;
                case "*":
                    return 4;
                case "/":
                    return 5;
                case "==":
                    return 6;
                case "!=":
                    return 7;
                case "<=":
                    return 8;
                case ">=":
                    return 9;
                case ">":
                    return 10;
                case "<":
                    return 11;
                case "!":
                    return 12;
                case "&&":
                    return 13;
                case "||":
                    return 14;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}