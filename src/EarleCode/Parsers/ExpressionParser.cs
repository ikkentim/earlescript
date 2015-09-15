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

using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using EarleCode.Blocks;
using EarleCode.Functions;
using EarleCode.Tokens;

namespace EarleCode.Parsers
{
    public class ExpressionParser : Parser<IExpression>
    {
        #region Overrides of Parser<IExpression>

        public override IExpression Parse(ICompiler compiler, IScriptScope scriptScope, ITokenizer tokenizer)
        {
            Debug.WriteLine($"Parsing expression... current token is {tokenizer.Current}");

            IExpression expression;
            switch (tokenizer.Current.Type)
            {
                case TokenType.Identifier:
                    // Check for reserved keywords
                    switch (tokenizer.Current.Value.ToLower())
                    {
                        case "true":
                            expression = new ValueExpression(scriptScope, new EarleValue(1));
                            tokenizer.AssertMoveNext();
                            break;
                        case "false":
                            expression = new ValueExpression(scriptScope, new EarleValue(0));
                            tokenizer.AssertMoveNext();
                            break;
                        case "null":
                            expression = new ValueExpression(scriptScope, EarleValue.Null);
                            tokenizer.AssertMoveNext();
                            break;
                        default:
                            if (compiler.Grammar.Matches(tokenizer, "FUNCTION_CALL"))
                            {
                                var functionCallParser = new FunctionCallParser();
                                expression = functionCallParser.Parse(compiler, scriptScope, tokenizer);
                            }
                            else
                            {
                                var name = tokenizer.Current.Value;
                                tokenizer.AssertMoveNext();

                                var indexers = new List<IExpression>();

                                while (tokenizer.Current.Is(TokenType.Token, "["))
                                {
                                    tokenizer.AssertMoveNext();
                                    indexers.Add(Parse(compiler, scriptScope, tokenizer));
                                    tokenizer.SkipToken("]", TokenType.Token);
                                }

                                expression = new VariableExpression(scriptScope, name, indexers.ToArray());
                            }
                            break;
                    }
                    break;
                case TokenType.NumberLiteral:
                    float fValue;
                    int iValue;
                    if (int.TryParse(tokenizer.Current.Value, out iValue))
                        expression = new ValueExpression(scriptScope, new EarleValue(iValue));
                    else if (float.TryParse(tokenizer.Current.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out fValue))
                        expression = new ValueExpression(scriptScope, new EarleValue(fValue));
                    else
                        throw new ParseException(tokenizer.Current, "Failed to parse number");
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
                        tokenizer.SkipToken(")", TokenType.Token);
                    }
                    else if (compiler.Grammar.Matches(tokenizer, "FUNCTION_CALL"))
                    {
                        var functionCallParser = new FunctionCallParser();
                        expression = functionCallParser.Parse(compiler, scriptScope, tokenizer);
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
                        tokenizer.SkipToken("::", TokenType.Token);
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
                    else
                        throw new ParseException(tokenizer.Current, "Unexpected token");
                    break;
                default:
                    throw new ParseException(tokenizer.Current, "Unexpected token type");
            }

            // Check for ops
            if (tokenizer.Current.Type == TokenType.Token &&
                new[] {"+", "-", "&&", "||"}.Contains(tokenizer.Current.Value))
            {
                var operatorToken = tokenizer.Current.Value;
                tokenizer.AssertMoveNext();
                return new OperatorExpression(scriptScope, expression, operatorToken,
                    Parse(compiler, scriptScope, tokenizer));
            }

            return expression;
        }

        #endregion
    }
}