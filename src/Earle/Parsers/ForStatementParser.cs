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

using Earle.Blocks;
using Earle.Blocks.Expressions;
using Earle.Tokens;

namespace Earle.Parsers
{
    public class ForStatementParser : Parser<ForStatement>
    {
        private readonly AssignmentParser _assignmentParser = new AssignmentParser();
        private readonly ExpressionParser _expressionParser = new ExpressionParser();

        public ForStatementParser()
            : base("STATEMENT_FOR", true)
        {
        }

        #region Overrides of Parser<IfStatement>

        public override ForStatement Parse(Block parent, Tokenizer tokenizer)
        {
            SkipToken(tokenizer, "for", TokenType.Identifier);
            SkipToken(tokenizer, "(", TokenType.Token);

            Assignment init = null;
            Expression cond = null;
            Assignment incr = null;

            // Initialize
            if (!tokenizer.Current.Is(TokenType.Token, ";"))
            {
                if (!Compiler.Grammar.Matches(tokenizer, _assignmentParser.ParserRule))
                    throw new ParseException(tokenizer.Current, "Unexpected token");

                init = _assignmentParser.Parse(parent, tokenizer);
            }

            SkipToken(tokenizer, ";", TokenType.Token);

            // Condition
            if (!tokenizer.Current.Is(TokenType.Token, ";"))
            {
                if (!Compiler.Grammar.Matches(tokenizer, _expressionParser.ParserRule))
                    throw new ParseException(tokenizer.Current, "Unexpected token");

                cond = _expressionParser.Parse(parent, tokenizer);
            }

            SkipToken(tokenizer, ";", TokenType.Token);

            // Increment
            if (!tokenizer.Current.Is(TokenType.Token, ")"))
            {
                if (!Compiler.Grammar.Matches(tokenizer, _assignmentParser.ParserRule))
                    throw new ParseException(tokenizer.Current, "Unexpected token");

                incr = _assignmentParser.Parse(parent, tokenizer);
            }

            SkipToken(tokenizer, ")", TokenType.Token);

            return new ForStatement(parent, init, cond, incr);
        }

        #endregion
    }
}