using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EarleCode.Retry;
using EarleCode.Retry.Instructions;
using EarleCode.Retry.Lexing;

namespace EarleCode.Retry.Parsers
{
    public class AssignmentExpressionParser : Parser
    {
        #region Implementation of IParser

        protected override void Parse()
        {
            // NAME = EXPRESSION;

            Lexer.AssertToken(TokenType.Identifier);

            var name = Lexer.Current.Value;
            Lexer.AssertMoveNext();

            Lexer.SkipToken(TokenType.Token, "=");

            Parse<ExpressionParser>();
            PushReference(null, name);
            Yield(OpCode.Write);
        }

        #endregion
    }
}
