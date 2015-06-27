using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Earle.Variables;

namespace Earle.Blocks
{
    public class Return : Block
    {
        private readonly Expression _expression;

        public Return(Block parent, Expression expression) : base(parent)
        {
            _expression = expression;
        }

        #region Overrides of Block

        public override bool IsReturnStatement
        {
            get { return true; }
        }

        public override ValueContainer Run()
        {
            return _expression.Run();
        }

        #endregion
    }
}
