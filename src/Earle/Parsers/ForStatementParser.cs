using System.Collections.Generic;
using Earle.Blocks;
using Earle.Blocks.Expressions;
using Earle.Tokens;
using Earle.Variables;

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
                if(!Compiler.Grammar.Matches(tokenizer, _assignmentParser.ParserRule))
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