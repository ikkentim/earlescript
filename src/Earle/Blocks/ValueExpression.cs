using Earle.Variables;

namespace Earle.Blocks
{
    public class ValueExpression : Expression
    {
        private readonly ValueContainer _value;

        public ValueExpression(Block parent, ValueContainer value) : base(parent)
        {
            _value = value;
        }

        #region Overrides of Block

        public override ValueContainer Run()
        {
            return _value;
        }

        #endregion
    }
}