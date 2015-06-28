using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Earle.Variables;

namespace Earle.Blocks
{
    public class UnaryOperatorExpression : Expression
    {
        private static readonly Operator[] Operators =
        {
            // Number
            new Operator("+", VarType.Number, v => v),
            new Operator("-", VarType.Number, v => -(float) v),
        };

        public UnaryOperatorExpression(Block parent) : base(parent)
        {
        }

        public UnaryOperatorExpression(Block parent, string op, Expression expression)
            : base(parent)
        {
            OP = op;
            Value = expression;
        }

        public string OP { get; set; }
        public Expression Value { get; set; }

        #region Overrides of Block

        public override ValueContainer Run()
        {
            var value = Value == null ? new ValueContainer(VarType.Null, null) : Value.Run();
            if (value == null)
                return new ValueContainer(VarType.Null, null);

            var op = Operators.FirstOrDefault(o => o.Type == value.Type && o.OpString == OP);

            if (op == null)
                throw new RuntimeException(string.Format("Unsupported {1} operator `{0}` called", OP, value.Type));

            return op.Calculate(value);
        }

        #endregion

        private class Operator
        {
            private readonly Func<ValueContainer, ValueContainer> _function;

            public Operator(string opString, VarType type,
                Func<ValueContainer, ValueContainer> function)
            {
                if (function == null) throw new ArgumentNullException("function");
                _function = function;
                OpString = opString;
                Type = type;
            }

            public string OpString { get; private set; }
            public VarType Type { get; private set; }

            public ValueContainer Calculate(ValueContainer value)
            {
                if (value == null || value.Type == VarType.Null)
                    return new ValueContainer(VarType.Null, null);

                return _function(value);
            }
        }
    }
}
