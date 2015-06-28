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

using Earle.Blocks;
using Earle.Tokens;

namespace Earle.Parsers
{
    public class WhileStatementParser : Parser<WhileStatement>
    {
        private readonly ExpressionParser _expressionParser = new ExpressionParser();

        public WhileStatementParser()
            : base("STATEMENT_WHILE", true)
        {
        }

        #region Overrides of Parser<IfStatement>

        public override WhileStatement Parse(Block parent, Tokenizer tokenizer)
        {
            SkipToken(tokenizer, "while", TokenType.Identifier);
            SkipToken(tokenizer, "(", TokenType.Token);
            var expression = _expressionParser.Parse(parent, tokenizer);
            SkipToken(tokenizer, ")", TokenType.Token);

            return new WhileStatement(parent, expression);
        }

        #endregion
    }
}