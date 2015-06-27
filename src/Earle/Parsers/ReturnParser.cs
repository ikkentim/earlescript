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
            var token = tokenizer.Current;

            if (token.Type != TokenType.Identifier || token.Value != "return")
                throw new ParseException(token, "Unexpected token");

            if(!tokenizer.MoveNext())
                throw new ParseException(-1, -1, "Unexpected end of file");

            // TODO return is parent of expression
            return new Return(parent, _expressionParser.Parse(parent, tokenizer));
        }

        #endregion
    }
}
