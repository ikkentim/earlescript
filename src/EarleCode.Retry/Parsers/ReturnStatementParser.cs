using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EarleCode.Retry.Instructions;
using EarleCode.Retry.Lexing;

namespace EarleCode.Retry.Parsers
{
    public class ReturnStatementParser : Parser
    {
        #region Overrides of Parser

        protected override void Parse()
        {
            Lexer.SkipToken(TokenType.Identifier, "return");

            if (Lexer.Current.Is(TokenType.Token, ";"))
                Yield(OpCode.PushNull);
            else
                Parse<ExpressionParser>();

            Yield(OpCode.Return);
        }

        #endregion
    }
}
