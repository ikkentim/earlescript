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