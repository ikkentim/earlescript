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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using EarleCode.Blocks;
using EarleCode.Blocks.Expressions;
using EarleCode.Functions;
using EarleCode.Tokens;
using EarleCode.Values;

namespace EarleCode.Parsers
{
    public class ExpressionParser : Parser<IExpression>
    {
        private readonly string[] _supportedBinaryOperators =
        {
            "+", "-", "&&", "||", "<", ">", "<=", ">=", "==", "!="
        };

        #region Overrides of Parser<IExpression>

        public override IExpression Parse(ICompiler compiler, IScriptScope scriptScope, ITokenizer tokenizer)
        {
            IExpression expression;

            if (compiler.Grammar.Matches(tokenizer, "ASSIGNMENT_UNARY"))
                expression = ParserUtilities.Delegate<AssignmentUnaryExpressionParser, IExpression>(compiler,
                    scriptScope, tokenizer);
            else if (compiler.Grammar.Matches(tokenizer, "ASSIGNMENT"))
                expression = ParserUtilities.Delegate<AssignmentExpressionParser, IExpression>(compiler, scriptScope,
                    tokenizer);
            else if (compiler.Grammar.Matches(tokenizer, "FUNCTION_CALL"))
                expression = ParserUtilities.Delegate<FunctionCallParser, IExpression>(compiler, scriptScope, tokenizer);
            else if (compiler.Grammar.Matches(tokenizer, "VECTOR"))
                expression = ParserUtilities.Delegate<VectorExpressionParser, IExpression>(compiler, scriptScope,
                    tokenizer);
            else if (compiler.Grammar.Matches(tokenizer, "KEYWORD"))
            {
                tokenizer.AssertToken(TokenType.Identifier);
                expression = new ValueExpression(scriptScope, KeywordToValue(tokenizer.Current.Value));
                tokenizer.AssertMoveNext();
            }
            else
            {
                switch (tokenizer.Current.Type)
                {
                    case TokenType.Identifier:
                    {
                        var name = tokenizer.Current.Value;
                        tokenizer.AssertMoveNext();

                        var indexers = new List<IExpression>();

                        while (tokenizer.Current.Is(TokenType.Token, "["))
                        {
                            tokenizer.AssertMoveNext();
                            indexers.Add(Parse(compiler, scriptScope, tokenizer));
                            tokenizer.SkipToken(TokenType.Token, "]");
                        }

                        expression = new VariableExpression(scriptScope, new VariableNameExpression(scriptScope, name, indexers.ToArray()));
                    }
                        break;
                    case TokenType.NumberLiteral:
                        expression = new ValueExpression(scriptScope, ParserUtilities.ParseNumber(tokenizer));
                        tokenizer.AssertMoveNext();
                        break;
                    case TokenType.StringLiteral:
                        expression = new ValueExpression(scriptScope, new EarleValue(tokenizer.Current.Value));
                        tokenizer.AssertMoveNext();
                        break;
                    case TokenType.Token:
                        if (tokenizer.Current.Value == "(")
                        {
                            tokenizer.AssertMoveNext();
                            expression = Parse(compiler, scriptScope, tokenizer);
                            tokenizer.SkipToken(TokenType.Token, ")");
                        }
                        else if (tokenizer.Current.Value == "::" || tokenizer.Current.Value == "\\")
                        {
                            var path = "";
                            while (tokenizer.Current.Is(TokenType.Token, "\\"))
                            {
                                tokenizer.AssertMoveNext();
                                tokenizer.AssertToken(TokenType.Identifier);
                                path += "\\" + tokenizer.Current.Value;
                            }
                            tokenizer.SkipToken(TokenType.Token, "::");
                            tokenizer.AssertToken(TokenType.Identifier);
                            var name = tokenizer.Current.Value;
                            expression = new ValueExpression(scriptScope,
                                new EarleValue(new EarleFunctionSignature(name, path)));
                            tokenizer.AssertMoveNext();
                        }
                        else if (compiler.Grammar.Matches(tokenizer, "OPERATOR_UNARY"))
                        {
                            var unaryOperatorToken = tokenizer.Current.Value;
                            tokenizer.AssertMoveNext();
                            expression = new UnaryOperatorExpression(scriptScope, unaryOperatorToken,
                                Parse(compiler, scriptScope, tokenizer));
                        }
                        else if (compiler.Grammar.Matches(tokenizer, "OPERATOR_MOD_UNARY"))
                        {
                            var unaryOperatorToken = tokenizer.Current.Value;
                            tokenizer.AssertMoveNext();
                            expression = new UnaryOperatorExpression(scriptScope, unaryOperatorToken,
                                Parse(compiler, scriptScope, tokenizer));
                        }
                        else
                            throw new ParseException(tokenizer.Current, "Unexpected token");
                        break;
                    default:
                        throw new ParseException(tokenizer.Current, "Unexpected token type");
                }
            }


            // Check for operators
            if (tokenizer.Current.Is(TokenType.Token) && _supportedBinaryOperators.Contains(tokenizer.Current.Value))
            {
                var operatorToken = tokenizer.Current.Value;
                tokenizer.AssertMoveNext();
                return new BinaryOperatorExpression(scriptScope, expression, operatorToken,
                    Parse(compiler, scriptScope, tokenizer));
            }

            return expression;
        }

        #endregion

        private static EarleValue KeywordToValue(string keyword)
        {
            switch (keyword.ToLower())
            {
                case "true":
                    return 1;
                case "false":
                    return 0;
                case "null":
                    return EarleValue.Null;
                default:
                    throw new Exception("Unknown keyword");
            }
        }
    }
}