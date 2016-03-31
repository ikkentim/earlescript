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
using System.Globalization;
using System.Linq;
using EarleCode.Instructions;
using EarleCode.Lexing;

namespace EarleCode.Parsers
{
    public class ExpressionParser : Parser
    {
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

                    if (!EarleOperators.BinaryOperators.ContainsKey(op))
                        ThrowUnexpectedToken("-OPERATOR-");

                    if (operators.Any() && EarleOperators.BinaryOperators[operators.Peek()] >= EarleOperators.BinaryOperators[op])
                        do YieldBinaryOperator(operators.Pop()); while (operators.Any());

                    operators.Push(op);
                    Lexer.AssertMoveNext();
                }
                else
                {
                    if (operators.Any())
                        YieldBinaryOperator(operators.Pop());
                    break;
                }
            }

            if (operators.Any())
            {
                ThrowParseException("Invalid expression");
            }
        }

        #endregion

        protected virtual void YieldBinaryOperator(string op)
        {
            PushReference(null, $"operator{op}");
            Yield(OpCode.Call);
            Yield(2);
        }

        private void ParseValue()
        {
            string unaryOperator = null;

            // Sub-parsers (unary operators are not applicable)
            if (SyntaxMatches("ASSIGNMENT"))
            {
                Parse<AssignmentExpressionParser>();
                return;
            }

            if (SyntaxMatches("FUNCTION_CALL"))
            {
                Parse<CallExpressionParser>();
                return;
            }

            if (SyntaxMatches("EXPLICIT_FUNCTION_IDENTIFIER"))
            {
                Parse<FunctionReferenceExpressionParser>();
                return;
            }


            // Unary operator parsing
            if (SyntaxMatches("OPERATOR_UNARY"))
            {
                Lexer.AssertToken(TokenType.Token);
                unaryOperator = Lexer.Current.Value;
                Lexer.AssertMoveNext();

                if(!EarleOperators.IsUnaryOpertorTargetValid(unaryOperator, Lexer.Current.Type))
                    ThrowUnexpectedToken("Unexpected target for operator " + unaryOperator);
            }

            if (SyntaxMatches("KEYWORD"))
                ParseKeyword();
            else if (SyntaxMatches("VECTOR"))
            {
                var vectorSize = 3;
                Lexer.SkipToken(TokenType.Token, "(");
                Parse<ExpressionParser>();
                Lexer.SkipToken(TokenType.Token, ",");
                Parse<ExpressionParser>();
                if (Lexer.Current.Is(TokenType.Token, ")"))
                {
                    vectorSize = 2;
                }
                else
                {
                    Lexer.SkipToken(TokenType.Token, ",");
                    Parse<ExpressionParser>();
                }

                PushReference(null, $"createVector{vectorSize}");
                Yield(OpCode.Call);
                Yield(vectorSize);

                Lexer.SkipToken(TokenType.Token, ")");
            }
            else if (Lexer.Current.Is(TokenType.Token, "("))
            {
                Lexer.AssertMoveNext();
                Parse<ExpressionParser>();
                Lexer.SkipToken(TokenType.Token, ")");
            }
            else if (Lexer.Current.Is(TokenType.NumberLiteral) || Lexer.Current.Is(TokenType.StringLiteral))
            {
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
                YieldUnaryOperator(unaryOperator);
        }

        private void YieldUnaryOperator(string op)
        {
            switch (op)
            {
                case "-":
                    PushInteger(-1);
                    YieldBinaryOperator("*");
                    break;
                case "!":
                    Yield(OpCode.Not);
                    break;
                default:
                    PushReference(null, $"operator{op}");
                    Yield(OpCode.Call);
                    Yield(1);
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
                case "undefined":
                    Yield(OpCode.PushUndefined);
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
                    else if (float.TryParse(Lexer.Current.Value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out fValue))
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