using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Earle.Variables;

namespace Earle.Blocks
{
    public class Assignment : Block
    {
        private readonly string _name;
        private readonly Expression _value;

        public Assignment(Block parent, string name, Expression value) : base(parent)
        {
            _name = name;
            _value = value;
        }

        #region Overrides of Block

        public override bool IsReturnStatement
        {
            get { return false; }
        }

        public override ValueContainer Run()
        {
            var variable = ResolveVariable(_name);

            if (variable == null)
                AddVariable(_name, _value.Run());
            else
                variable.SetValue(_value.Run());

            return null;
        }

        #endregion
    }
}
