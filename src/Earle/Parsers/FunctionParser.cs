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

using System.Collections.Generic;
using Earle.Blocks;
using Earle.Tokens;

namespace Earle.Parsers
{
    public class FunctionParser : Parser<Function>
    {
        public FunctionParser()
            : base("FUNCTION_DECLARATION", true)
        {
        }

        #region Overrides of Parser<Function>

        public override Function Parse(Block parent, Tokenizer tokenizer)
        {
            var name = tokenizer.Current.Value;
            var parameters = new List<string>();

            MoveNext(tokenizer);

            SkipToken(tokenizer, "(", TokenType.Token);

            if (!tokenizer.Current.Is(TokenType.Token, ")"))
                while (true)
                {
                    if (tokenizer.Current.Type == TokenType.Identifier)
                        parameters.Add(tokenizer.Current.Value);

                    SkipToken(tokenizer, TokenType.Identifier);

                    if (!tokenizer.Current.Is(TokenType.Token, ","))
                        break;

                    MoveNext(tokenizer);
                }

            SkipToken(tokenizer, ")", TokenType.Token);

            return new Function(parent, name, parameters.ToArray());
        }

        #endregion
    }
}