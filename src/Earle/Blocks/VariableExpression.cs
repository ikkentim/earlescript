using System;
using Earle.Variables;

namespace Earle.Blocks
{
    public class VariableExpression : Expression
    {
        private readonly string _name;

        public VariableExpression(Block parent, string name) : base(parent)
        {
            if (name == null) throw new ArgumentNullException("name");
            _name = name;
        }

        #region Overrides of Block

        public override ValueContainer Run()
        {
            return ResolveVariable(_name);
        }

        #endregion
    }
}