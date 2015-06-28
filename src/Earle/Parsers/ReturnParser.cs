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

        public ReturnParser()
            : base(false, "STATEMENT_RETURN")
        {
        }

        #region Overrides of Parser<Return>

        public override Return Parse(Block parent, Tokenizer tokenizer)
        {
            SkipToken(tokenizer, "return", TokenType.Identifier);

            return tokenizer.Current.Is(TokenType.Token, ";")
                ? new Return(parent, null)
                : new Return(parent, _expressionParser.Parse(parent, tokenizer));
        }

        #endregion
    }
}
