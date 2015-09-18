using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EarleCode.Blocks;
using EarleCode.Tokens;

namespace EarleCode.Parsers
{
    public class VectorExpressionParser : Parser<VectorExpression>
    {
        #region Overrides of Parser<VectorExpression>

        public override VectorExpression Parse(ICompiler compiler, IScriptScope scriptScope, ITokenizer tokenizer)
        {
            var expressionParser = new ExpressionParser();

            tokenizer.SkipToken(TokenType.Token, "(");
            var x = expressionParser.Parse(compiler, scriptScope, tokenizer);
            tokenizer.SkipToken(TokenType.Token, ",");
            var y = expressionParser.Parse(compiler, scriptScope, tokenizer);
            tokenizer.SkipToken(TokenType.Token, ",");
            var z = expressionParser.Parse(compiler, scriptScope, tokenizer);
            tokenizer.SkipToken(TokenType.Token, ")");

            return new VectorExpression(scriptScope, x, y, z);
        }

        #endregion
    }
}
