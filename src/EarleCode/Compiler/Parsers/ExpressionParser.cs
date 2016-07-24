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
using EarleCode.Compiler.Lexing;
using EarleCode.Runtime;
using EarleCode.Runtime.Instructions;

namespace EarleCode.Compiler.Parsers
{
    public class ExpressionParser : Parser
    {
        private string GetOperator(string[] opList)
        {
            var str = "";
            do
            {
                str += Lexer.Current.Value;
                Lexer.AssertMoveNext();
            } while (Lexer.Current.Is(TokenType.Token) && opList.Any(o => o.StartsWith(str)) &&
                     opList.Where(o => o.StartsWith(str)).Max(o => o.Length) > str.Length);

            return str;
        }

        #region Overrides of Parser

        protected override void Parse()
        {
            // Output:
            // ?

            var operators = new Stack<string>();

            for (;;)
            {
                ParseValue();
                

                if (SyntaxMatches("OPERATOR_AND"))
                {
                    Lexer.SkipToken(TokenType.Token, "&");
                    Lexer.SkipToken(TokenType.Token, "&");

                    var block = ParseToBuffer<ExpressionParser>(false, false);
                    PushJump(false, block.Length + 5);
                    Yield(block);
                    PushJump(5);
                    PushInteger(0);

                    return;
                }

                if (SyntaxMatches("OPERATOR_OR"))
                {
                    Lexer.SkipToken(TokenType.Token, "|");
                    Lexer.SkipToken(TokenType.Token, "|");

                    var block = ParseToBuffer<ExpressionParser>();
                    PushJump(true, block.Length + 5);
                    Yield(block);
                    PushJump(5);
                    PushInteger(0);

                    return;
                }

                if (SyntaxMatches("OPERATOR"))
                {
                    Lexer.AssertToken(TokenType.Token);

                    var op = GetOperator(EarleOperators.BinaryOperators.Keys.ToArray());

                    if (op.Length == 0)
                        ThrowUnexpectedToken("-OPERATOR-");

                    if (operators.Any() &&
                        EarleOperators.BinaryOperators[operators.Peek()] >= EarleOperators.BinaryOperators[op])
                        do YieldBinaryOperator(operators.Pop()); while (operators.Any());

                    operators.Push(op);
                }
                else
                {
                    YieldBinaryOperators(operators);
                    return;
                }
            }
        }

        #endregion

        #region Parse

        protected virtual bool ParseFunctionCall()
        {
            Parse<CallExpressionParser>();
            Parse<DereferenceParser>();

            return true;
        }

        protected virtual void ParseValue()
        {
            string unaryOperator = null;

            // Sub-parsers (unary operators are not applicable)
            if (SyntaxMatches("ASSIGNMENT"))
            {
                Parse<AssignmentExpressionParser>();
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

                if (!EarleOperators.IsUnaryOpertorTargetValid(unaryOperator, Lexer.Current.Type))
                    ThrowUnexpectedToken("Unexpected target for operator " + unaryOperator);
            }

            if (SyntaxMatches("FUNCTION_CALL") && ParseFunctionCall())
            {
                // If parse is not allowed, move on to next else-if case
            }
            else if (SyntaxMatches("KEYWORD"))
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

                PushCallWithoutTarget(null, $"createVector{vectorSize}", vectorSize);

                Lexer.SkipToken(TokenType.Token, ")");
            }
            else if (Lexer.Current.Is(TokenType.Token, "("))
            {
                Lexer.AssertMoveNext();
                Parse<ExpressionParser>();
                Lexer.SkipToken(TokenType.Token, ")");
                Parse<DereferenceParser>();
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
                Parse<DereferenceParser>();
                Yield(OpCode.Read);
            }
            else
                ThrowUnexpectedToken("-VALUE-");

            if (unaryOperator != null)
                YieldUnaryOperator(unaryOperator);
        }

        protected virtual void ParseKeyword()
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
                case "[":
                    Lexer.AssertMoveNext();
                    Lexer.AssertToken(TokenType.Token, "]");
                    Yield(OpCode.PushArray);
                    break;
                default:
                    ThrowUnexpectedToken("-KEYWORD-");
                    break;
            }
            Lexer.AssertMoveNext();
        }

        protected virtual void ParseLiteral()
        {
            switch (Lexer.Current.Type)
            {
                case TokenType.NumberLiteral:
                    float fValue;
                    int iValue;

                    if (int.TryParse(Lexer.Current.Value, out iValue))
                        PushInteger(iValue);
                    else if (float.TryParse(Lexer.Current.Value, NumberStyles.AllowDecimalPoint,
                        CultureInfo.InvariantCulture, out fValue))
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

        #endregion

        #region Yield

        protected virtual void YieldBinaryOperator(string op)
        {
            PushCallWithoutTarget(null, $"operator{op}", 2);
        }

        protected virtual void YieldBinaryOperators(Stack<string> operators)
        {
            while (operators.Count > 0)
                YieldBinaryOperator(operators.Pop());
        }

        protected virtual void YieldUnaryOperator(string op)
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
                    PushCallWithoutTarget(null, $"operator{op}", 1);
                    break;
            }
        }

        #endregion
    }
}