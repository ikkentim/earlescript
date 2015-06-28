using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Earle.Blocks;
using Earle.Tokens;

namespace Earle.Parsers
{
    public class ReturnParser : Parser<Return>
    {
        private readonly ExpressionParser _expressionParser = new ExpressionParser();

        #region Overrides of Parser<Return>

        public override string ParserRule
        {
            get { return "STATEMENT_RETURN"; }
        }

        public override Return Parse(Block parent, Tokenizer tokenizer)
        {
            SkipToken(tokenizer, "return", TokenType.Identifier);

            return new Return(parent, _expressionParser.Parse(parent, tokenizer));
        }

        #endregion
    }
}
