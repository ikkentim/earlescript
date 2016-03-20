using System;
using EarleCode.Blocks.Expressions;
using EarleCode.Values;

namespace EarleCode.Blocks.Expressions
{
    public class VectorExpression : Block, IExpression
    {
        private readonly IExpression _xExpression;
        private readonly IExpression _yExpression;
        private readonly IExpression _zExpression;

        public VectorExpression(IScriptScope scriptScope, IExpression xExpression, IExpression yExpression, IExpression zExpression) : base(scriptScope)
        {
            if (xExpression == null) throw new ArgumentNullException(nameof(xExpression));
            if (yExpression == null) throw new ArgumentNullException(nameof(yExpression));
            if (zExpression == null) throw new ArgumentNullException(nameof(zExpression));
            _xExpression = xExpression;
            _yExpression = yExpression;
            _zExpression = zExpression;
        }

        #region Overrides of Block

        public override InvocationResult Invoke(Runtime runtime, IEarleContext context)
        {
            var values = new EarleValue[2];
            var z = EarleValue.Null;
            InvocationResult result;

            if (!ExpressionUtility.Invoke(_xExpression, 0, runtime, context, out result, ref values[0]) ||
                !ExpressionUtility.Invoke(_xExpression, 1, runtime, context, out result, ref values[1]) ||
                !ExpressionUtility.Invoke(_xExpression, 2, runtime, context, out result, ref z))
            {
                result.IncompleteResult.Data = values;
                return result;
            }

            return new InvocationResult(InvocationState.None,
                new EarleVector(values[0].Cast<float>(), values[1].Cast<float>(), z.Cast<float>()));
        }

        public override InvocationResult Continue(Runtime runtime, IncompleteInvocationResult incompleteInvocationResult)
        {
            var values = incompleteInvocationResult.Data;
            var z = EarleValue.Null;
            var result = InvocationResult.Empty;

            if (!ExpressionUtility.Continue(_xExpression, 0, runtime, incompleteInvocationResult, ref result, ref values[0]) ||
                !ExpressionUtility.Continue(_xExpression, 1, runtime, incompleteInvocationResult, ref result, ref values[1]) ||
                !ExpressionUtility.Continue(_xExpression, 2, runtime, incompleteInvocationResult, ref result, ref z))
            {
                result.IncompleteResult.Data = values;
                return result;
            }

            return new InvocationResult(InvocationState.None,
                new EarleVector(values[0].Cast<float>(), values[1].Cast<float>(), z.Cast<float>()));
        }

        #endregion

        #region Overrides of Object
        
        public override string ToString()
        {
            return $"({_xExpression}, {_yExpression}, {_zExpression})";
        }

        #endregion
    }
}