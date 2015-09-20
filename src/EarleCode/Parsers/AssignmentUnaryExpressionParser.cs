using System.Collections.Generic;
using EarleCode.Blocks;
using EarleCode.Blocks.Expressions;
using EarleCode.Tokens;

namespace EarleCode.Parsers
{
    public class AssignmentUnaryExpressionParser : Parser<AssignmentUnaryExpression>
    {
        #region Overrides of Parser<AssignmentExpression>

        public override AssignmentUnaryExpression Parse(ICompiler compiler, IScriptScope scriptScope, ITokenizer tokenizer)
        {
            var isPostOperation = true;
            string operatorToken = null;

            if (tokenizer.Current.Is(TokenType.Token, "--", "++"))
            {
                isPostOperation = false;
                operatorToken = tokenizer.Current.Value;
                tokenizer.AssertMoveNext();
            }

            tokenizer.AssertToken(TokenType.Identifier);

            var name = tokenizer.Current.Value;
            var indexers = new List<IExpression>();

            tokenizer.AssertMoveNext();

            var expressionParser = new ExpressionParser();
            while (tokenizer.Current.Is(TokenType.Token, "["))
            {
                indexers.Add(expressionParser.Parse(compiler, scriptScope, tokenizer));
                tokenizer.SkipToken(TokenType.Token, "]");
            }

            if (isPostOperation)
            {
                tokenizer.AssertToken(TokenType.Token, "--", "++");
                operatorToken = tokenizer.Current.Value;
                tokenizer.AssertMoveNext();
            }
            return new AssignmentUnaryExpression(scriptScope, name, indexers.ToArray(), operatorToken, isPostOperation);
        }

        #endregion
    }
}