﻿// EarleCode
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
using EarleCode.Blocks;
using EarleCode.Functions;
using EarleCode.Tokens;
using static System.Diagnostics.Debug;

namespace EarleCode.Parsers
{
    public class FunctionCallParser : Parser<FunctionCall>
    {
        #region Overrides of Parser<FunctionCall>

        public override FunctionCall Parse(ICompiler compiler, IScriptScope scriptScope, ITokenizer tokenizer)
        {
            WriteLine("Parsing function call...");

            string path = null;

            if (tokenizer.Current.Is(TokenType.Token, "::"))
            {
                tokenizer.SkipToken("::", TokenType.Token);
            }
            else if (tokenizer.Current.Is(TokenType.Token, "\\"))
            {
                // Construct path to the function
                var identifier = false;
                path = "";
                do
                {
                    if (identifier)
                        tokenizer.AssertToken(TokenType.Identifier);
                    else
                        tokenizer.AssertToken("\\", TokenType.Token);
                    identifier = !identifier;

                    path += tokenizer.Current.Value;

                    tokenizer.AssertMoveNext();
                } while (!tokenizer.Current.Is(TokenType.Token, "::"));

                tokenizer.SkipToken("::", TokenType.Token);
            }

            tokenizer.AssertToken(TokenType.Identifier);
            var name = tokenizer.Current.Value;
            tokenizer.AssertMoveNext();

            tokenizer.SkipToken("(", TokenType.Token);

            var arguments = new List<IExpression>();
            while (!tokenizer.Current.Is(TokenType.Token, ")"))
            {
                var expressionParser = new ExpressionParser();
                arguments.Add(expressionParser.Parse(compiler, scriptScope, tokenizer));
            }

            var functionCall = new FunctionCall(scriptScope, new EarleFunctionSignature(name, path), arguments.ToArray());
            compiler.Compile(functionCall, tokenizer);
            tokenizer.AssertMoveNext();
            return functionCall;
        }

        #endregion
    }
}