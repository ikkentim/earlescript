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

using EarleCode.Blocks;
using EarleCode.Blocks.Expressions;
using EarleCode.Blocks.Statements;
using EarleCode.Tokens;

namespace EarleCode.Parsers
{
    public class StatementWhileParser : Parser<StatementWhile>
    {
        #region Overrides of Parser<StatementIf>

        public override StatementWhile Parse(ICompiler compiler, IScriptScope scriptScope, ITokenizer tokenizer)
        {
            tokenizer.SkipToken(TokenType.Identifier, "while");
            tokenizer.SkipToken(TokenType.Token, "(");

            var expression = ParserUtilities.Delegate<ExpressionParser, IExpression>(compiler, scriptScope, tokenizer);
            tokenizer.SkipToken(TokenType.Token, ")");

            var statement = new StatementWhile(scriptScope, expression);
            compiler.CompileToTarget(statement, tokenizer);

            return statement;
        }

        #endregion
    }
    public class StatementForParser : Parser<StatementFor>
    {
        #region Overrides of Parser<StatementIf>

        public override StatementFor Parse(ICompiler compiler, IScriptScope scriptScope, ITokenizer tokenizer)
        {
            tokenizer.SkipToken(TokenType.Identifier, "for");
            tokenizer.SkipToken(TokenType.Token, "(");

            IBlock assignment = null;
            if (!tokenizer.Current.Is(TokenType.Token, ";"))
            {
                switch (compiler.Grammar.GetMatch(tokenizer))
                {
                    case "ASSIGNMENT":
                        assignment = ParserUtilities.Delegate<AssignmentExpressionParser>(compiler, scriptScope, tokenizer);
                        break;
                    case "ASSIGNMENT_UNARY":
                        assignment = ParserUtilities.Delegate<AssignmentUnaryExpressionParser>(compiler, scriptScope, tokenizer);
                        break;
                }
            }

            tokenizer.SkipToken(TokenType.Token, ";");

            var check = tokenizer.Current.Is(TokenType.Token, ";")
                ? null
                : ParserUtilities.Delegate<ExpressionParser, IExpression>(compiler, scriptScope, tokenizer);

            tokenizer.SkipToken(TokenType.Token, ";");


            IBlock increment = null;
            if (!tokenizer.Current.Is(TokenType.Token, ")"))
            {
                switch (compiler.Grammar.GetMatch(tokenizer))
                {
                    case "ASSIGNMENT":
                        increment = ParserUtilities.Delegate<AssignmentExpressionParser>(compiler, scriptScope, tokenizer);
                        break;
                    case "ASSIGNMENT_UNARY":
                        increment = ParserUtilities.Delegate<AssignmentUnaryExpressionParser>(compiler, scriptScope, tokenizer);
                        break;
                }
            }
            
            tokenizer.SkipToken(TokenType.Token, ")");

            var statement = new StatementFor(scriptScope, assignment, check, increment);
            compiler.CompileToTarget(statement, tokenizer);

            return statement;
        }

        #endregion
    }
}