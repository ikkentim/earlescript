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

using System.Collections.Generic;
using System.Linq;
using EarleCode.Instructions;
using EarleCode.Lexing;

namespace EarleCode.Parsers
{
    public class ExpressionParser : Parser
    {
        private readonly IDictionary<string, int> _binaryOperatorOrder = new Dictionary<string, int>
        {
            ["+"] = 1,
            ["-"] = 1,
            ["*"] = 2,
            ["/"] = 2
        };

        #region Overrides of Parser

        protected override void Parse()
        {
            var operators = new Stack<string>();

            for (;;)
            {
                ParseValue();

                if (SyntaxMatches("OPERATOR"))
                {
                    Lexer.AssertToken(TokenType.Token);
                    var op = Lexer.Current.Value;

                    if (!_binaryOperatorOrder.ContainsKey(op))
                        ThrowUnexpectedToken("-OPERATOR-");

                    if (operators.Any())
                    {
                        if (_binaryOperatorOrder[operators.Peek()] >= _binaryOperatorOrder[op])
                        {
                            do
                            {
                                ParseBinaryOperator(operators.Pop());
                            } while (operators.Any());
                        }
                    }

                    operators.Push(op);
                    Lexer.AssertMoveNext();
                }
                else
                {
                    if (operators.Any())
                        ParseBinaryOperator(operators.Pop());
                    break;
                }
            }

            if (operators.Any())
            {
                ThrowParseException("Invalid expression");
            }
        }

        #endregion

        private void ParseValue()
        {
            string unaryOperator = null;
            if (SyntaxMatches("OPERATOR_UNARY"))
            {
                Lexer.AssertToken(TokenType.Token);
                unaryOperator = Lexer.Current.Value;
                Lexer.AssertMoveNext();
            }

            if (SyntaxMatches("KEYWORD"))
                ParseKeyword();
            else if (Lexer.Current.Is(TokenType.NumberLiteral) || Lexer.Current.Is(TokenType.StringLiteral))
            {
                // TODO: If unaryOperator is set, reduce opcodes by not having to parse unary op.
                ParseLiteral();
            }
            else if (Lexer.Current.Is(TokenType.Identifier))
            {
                var name = Lexer.Current.Value;
                Lexer.AssertMoveNext();
                PushReference(null, name);
                Yield(OpCode.Read);
            }
            else
                ThrowUnexpectedToken("-VALUE-");

            if (unaryOperator != null)
                ParseUnaryOperator(unaryOperator);
        }

        private void ParseUnaryOperator(string op)
        {
            switch (op)
            {
                case "-":
                    PushInteger(-1);
                    Yield(OpCode.Multiply);
                    break;
                case "!":
                    Yield(OpCode.Not);
                    break;
                default:
                    ThrowUnexpectedToken("UNARY_OPERATOR");
                    break;
            }
        }

        private void ParseBinaryOperator(string op)
        {
            switch (op)
            {
                case "-":
                    Yield(OpCode.Subtract);
                    break;
                case "+":
                    Yield(OpCode.Add);
                    break;
                case "*":
                    Yield(OpCode.Multiply);
                    break;
                default:
                    ThrowUnexpectedToken("OPERATOR");
                    break;
            }
        }

        private void ParseKeyword()
        {
            switch (Lexer.Current.Value)
            {
                case "true":
                    PushInteger(1);
                    break;
                case "false":
                    PushInteger(0);
                    break;
                case "null":
                    Yield(OpCode.PushNull);
                    break;
                default:
                    ThrowUnexpectedToken("-KEYWORD-");
                    break;
            }
            Lexer.AssertMoveNext();
        }

        private void ParseLiteral()
        {
            switch (Lexer.Current.Type)
            {
                case TokenType.NumberLiteral:
                    float fValue;
                    int iValue;

                    if (int.TryParse(Lexer.Current.Value, out iValue))
                        PushInteger(iValue);
                    else if (float.TryParse(Lexer.Current.Value, out fValue))
                        PushFloat(fValue);
                    else
                        ThrowUnexpectedToken(TokenType.NumberLiteral);
                    break;
                case TokenType.StringLiteral:
                    PushString(Lexer.Current.Value);
                    break;
                default:
                    ThrowUnexpectedToken(TokenType.NumberLiteral, TokenType.StringLiteral);
                    break;
            }

            Lexer.AssertMoveNext();
        }
    }
}