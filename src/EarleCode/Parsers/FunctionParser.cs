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
using EarleCode.Functions;
using EarleCode.Retry.Parsers;
using EarleCode.Tokens;

namespace EarleCode.Parsers
{
    public class FunctionParser : Parser<EarleFunction>
    {
        #region Overrides of Parser<EarleFunction>

        public override EarleFunction Parse(ICompiler compiler, IScriptScope scriptScope, ITokenizer tokenizer)
        {
            var name = tokenizer.Current.Value;
            var parameters = new List<string>();

            tokenizer.AssertMoveNext();
            tokenizer.SkipToken(TokenType.Token, "(");

            if (!tokenizer.Current.Is(TokenType.Token, ")"))
                while (true)
                {
                    if (tokenizer.Current.Is(TokenType.Identifier))
                        parameters.Add(tokenizer.Current.Value);

                    tokenizer.SkipToken(TokenType.Identifier);

                    if (!tokenizer.Current.Is(TokenType.Token, ","))
                        break;

                    tokenizer.AssertMoveNext();
                }

            tokenizer.SkipToken(TokenType.Token, ")");

            var function = new EarleFunction(scriptScope, name, parameters.ToArray());
            compiler.CompileToTarget(function, tokenizer);
            return function;
        }

        #endregion
    }
}