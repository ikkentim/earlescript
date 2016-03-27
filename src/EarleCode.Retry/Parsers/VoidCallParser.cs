using System.Collections.Generic;
using EarleCode.Retry.Instructions;
using EarleCode.Retry.Lexing;

namespace EarleCode.Retry.Parsers
{
    public class VoidCallParser : BaseCallParser
    {
        #region Overrides of ExtendedParser<BaseCallParser>

        protected override void Parse()
        {
            base.Parse();
            Yield(OpCode.Pop);
        }

        #endregion
    }
}