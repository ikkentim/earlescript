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
using EarleCode.Lexing;

namespace EarleCode.Instructions
{
    [AttributeUsage(AttributeTargets.Field)]
    internal class OpCodeAttribute : Attribute
    {
        public OpCodeAttribute(string format) : this(format, null)
        {
        }

        public OpCodeAttribute(string format, Type instructionType)
        {
            Format = format;
            InstructionType = instructionType;
        }

        public string Format { get; }

        public Type InstructionType { get; }

        public string BuildString(byte[] pCode, ref int index)
        {
            var l = new Lexer(Format, Format);
            var str = "";
            var size = 1;
            while (l.MoveNext())
            {
                if (l.Current.Is(TokenType.Token, "$"))
                {
                    l.AssertMoveNext();
                    l.AssertToken(TokenType.Identifier);

                    switch (l.Current.Value)
                    {
                        case "int":
                            str += BitConverter.ToInt32(pCode, index + 1);
                            str += " ";
                            index += 4;
                            size += 4;
                            break;
                        case "float":
                            str += BitConverter.ToSingle(pCode, index + 1);
                            str += " ";
                            index += 4;
                            size += 4;
                            break;
                        case "string":
                            str += '"';
                            while (pCode[index + 1] != 0)
                            {
                                str += (char) pCode[index++ + 1];
                                size++;
                            }
                            str += "\" ";
                            index++;
                            size++;
                            break;
                        default:
                            throw new Exception();
                    }
                }
                else
                {
                    str += $"{l.Current.Value} ";
                }
            }

            return $"[{size}]" + str;
        }
    }
}