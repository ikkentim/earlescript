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

using System.Diagnostics;
using EarleCode.Blocks;
using EarleCode.Tokens;

namespace EarleCode.Parsers
{
    public class StatementIfParser : Parser<StatementIf>
    {
        #region Overrides of Parser<StatementIf>

        public override StatementIf Parse(ICompiler compiler, IScriptScope scriptScope, ITokenizer tokenizer)
        {
            Debug.WriteLine("PARSING IF.. At token " + tokenizer.Current);
            tokenizer.SkipToken("if", TokenType.Identifier);
            tokenizer.SkipToken("(", TokenType.Token);

            var expressionParser = new ExpressionParser();
            var expression = expressionParser.Parse(compiler, scriptScope, tokenizer);
            tokenizer.SkipToken(")", TokenType.Token);

            var statement = new StatementIf(scriptScope, expression);
            compiler.CompileBlock(statement, tokenizer);

            Debug.WriteLine("DONE PARSING IF.. At token " + tokenizer.Current);
            return statement;
        }

        #endregion
    }
}