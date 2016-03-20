using System.Collections.Generic;
using EarleCode.Retry.Instructions;
using EarleCode.Retry.Lexing;

namespace EarleCode.Retry.Parsers
{
    public class VoidCallParser : ExtendedParser<BaseCallParser>
    {
        #region Overrides of ExtendedParser<BaseCallParser>

        protected override IEnumerable<byte> ParseMore(Runtime runtime, Compiler compiler, EarleFile file, ILexer lexer)
        {
            yield return (byte)OpCode.Pop;
        }

        #endregion
    }
}