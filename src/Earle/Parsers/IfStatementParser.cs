using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Earle.Blocks;
using Earle.Tokens;

namespace Earle.Parsers
{
    public class IfStatementParser : Parser<IfStatement>
    {
        private readonly ExpressionParser _expressionParser = new ExpressionParser();
        public IfStatementParser() : base(true, "STATEMENT_IF")
        {
        }

        #region Overrides of Parser<IfStatement>

        public override IfStatement Parse(Block parent, Tokenizer tokenizer)
        {
            SkipToken(tokenizer, "if", TokenType.Identifier);
            SkipToken(tokenizer, "(", TokenType.Token);
            var expression = _expressionParser.Parse(parent, tokenizer);
            SkipToken(tokenizer, ")", TokenType.Token);

            return new IfStatement(parent, expression);
        }

        #endregion
    }
}
