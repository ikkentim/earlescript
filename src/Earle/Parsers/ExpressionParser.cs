﻿// Earle
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

using System.Collections.Generic;
using System.Linq;
using Earle.Blocks;
using Earle.Blocks.Expressions;
using Earle.Tokens;
using Earle.Variables;

namespace Earle.Parsers
{
    public class ExpressionParser : Parser<Expression>
    {
        private readonly FunctionCallParser _functionCallParser = new FunctionCallParser();

        public ExpressionParser()
            : base("EXPRESSION")
        {
        }

        #region Overrides of Parser<ExpressionParser>

        public override Expression Parse(Block parent, Tokenizer tokenizer)
        {
            Expression expression;
            switch (tokenizer.Current.Type)
            {
                case TokenType.Identifier:
                    // Check for reserved keywords
                    switch (tokenizer.Current.Value)
                    {
                        case "true":
                            expression = new ValueExpression(parent, new ValueContainer(1));
                            MoveNext(tokenizer);
                            break;
                        case "false":
                            expression = new ValueExpression(parent, new ValueContainer(0));
                            MoveNext(tokenizer);
                            break;
                        case "null":
                            expression = new ValueExpression(parent, new ValueContainer());
                            MoveNext(tokenizer);
                            break;
                        default:
                            var name = tokenizer.Current.Value;
                            MoveNext(tokenizer);

                            List<Expression> indexers = new List<Expression>();

                            while (tokenizer.Current.Is(TokenType.Token, "["))
                            {
                                MoveNext(tokenizer);
                                indexers.Add(Parse(parent, tokenizer));
                                SkipToken(tokenizer, "]", TokenType.Token);
                            }

                            expression = new VariableExpression(parent, name, indexers.ToArray());
                            break;
                    }
                    break;
                case TokenType.NumberLiteral:
                    float fValue;
                    int iValue;
                    if (int.TryParse(tokenizer.Current.Value, out iValue))
                        expression = new ValueExpression(parent, new ValueContainer(iValue));
                    else if (float.TryParse(tokenizer.Current.Value, out fValue))
                        expression = new ValueExpression(parent, new ValueContainer(fValue));
                    else
                        throw new ParseException(tokenizer.Current, "Failed to parse number");
                    MoveNext(tokenizer);
                    break;
                case TokenType.StringLiteral:
                    expression = new ValueExpression(parent, new ValueContainer(tokenizer.Current.Value));
                    MoveNext(tokenizer);
                    break;
                case TokenType.Token:
                    if (tokenizer.Current.Value == "(")
                    {
                        MoveNext(tokenizer);
                        expression = Parse(parent, tokenizer);
                        SkipToken(tokenizer, ")", TokenType.Token);
                    }
                    else if (Compiler.Grammar.Matches(tokenizer, "FUNCTION_CALL"))
                        expression = _functionCallParser.Parse(parent, tokenizer);
                    else if (tokenizer.Current.Value == "::" || tokenizer.Current.Value == "\\")
                    {
                        var path = "";
                        while (tokenizer.Current.Is(TokenType.Token, "\\"))
                        {
                            MoveNext(tokenizer);
                            AssertToken(tokenizer, TokenType.Identifier);
                            path += "\\" + tokenizer.Current.Value;
                        }
                        SkipToken(tokenizer, "::", TokenType.Token);
                        AssertToken(tokenizer, TokenType.Identifier);
                        var name = tokenizer.Current.Value;
                        expression = new ValueExpression(parent, new ValueContainer(new FunctionCall(parent, path, name)));
                        MoveNext(tokenizer);
                    }
                    else if (Compiler.Grammar.Matches(tokenizer, "OPERATOR_UNARY"))
                    {
                        var unaryop = tokenizer.Current.Value;
                        MoveNext(tokenizer);
                        expression = new UnaryOperatorExpression(parent, unaryop, Parse(parent, tokenizer));
                    }
                    else
                        throw new ParseException(tokenizer.Current, "Unexpected token");
                    break;
                default:
                    throw new ParseException(tokenizer.Current, "Unexpected token type");
            }

            // Check for ops
            if (tokenizer.Current.Type == TokenType.Token && /* token is operator */
                !new[] {",", ")", ";", "[", "]"}.Contains(tokenizer.Current.Value))
            {
                var optoken = tokenizer.Current.Value;
                MoveNext(tokenizer);
                return new OperatorExpression(parent, expression, optoken, Parse(parent, tokenizer));
            }

            return expression;
        }

        #endregion
    }
}