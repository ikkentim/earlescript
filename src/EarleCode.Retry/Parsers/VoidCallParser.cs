using System.Collections.Generic;
using EarleCode.Instructions;
using EarleCode.Lexing;

namespace EarleCode.Parsers
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