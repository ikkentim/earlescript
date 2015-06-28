// Earle
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
using Earle.Blocks;
using Earle.Blocks.Expressions;
using Earle.Tokens;

namespace Earle.Parsers
{
    internal class FunctionCallParser : Parser<FunctionCall>
    {
        public FunctionCallParser()
            : base(false, "FUNCTION_CALL")
        {
        }

        #region Overrides of Parser<FunctionCall>

        public override FunctionCall Parse(Block parent, Tokenizer tokenizer)
        {
            string path, name;
            if (tokenizer.Current.Is(TokenType.Token, "\\"))
            {
                path = string.Empty;
                do
                {
                    if (tokenizer.Current.Is(TokenType.Token, "\\") || tokenizer.Current.Type == TokenType.Identifier)
                        path += tokenizer.Current.Value;
                    else
                        throw new Exception();
                } while (tokenizer.MoveNext() && !tokenizer.Current.Is(TokenType.Token, ":"));

                SkipToken(tokenizer, ":", TokenType.Token);
                SkipToken(tokenizer, ":", TokenType.Token);
                AssertToken(tokenizer, TokenType.Identifier);

                name = tokenizer.Current.Value;
            }
            else if (tokenizer.Current.Type == TokenType.Identifier)
            {
                path = null;
                name = tokenizer.Current.Value;
            }
            else
            {
                throw new Exception();
            }

            MoveNext(tokenizer);
            SkipToken(tokenizer, "(", TokenType.Token);

            var arguments = new List<Expression>();
            var parser = new ExpressionParser();
            while (true)
            {
                arguments.Add(parser.Parse(parent, tokenizer));

                if (!tokenizer.Current.Is(TokenType.Token, ","))
                    break;

                MoveNext(tokenizer);
            }

            SkipToken(tokenizer, ")", TokenType.Token);

            return new FunctionCall(parent, path, name, arguments.ToArray());
        }

        #endregion
    }
}