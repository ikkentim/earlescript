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

            var xResult = _xExpression.Invoke(runtime, context);
            if (xResult.State == InvocationState.Incomplete)
                return new InvocationResult(new IncompleteInvocationResult(context, xResult.IncompleteResult) { Stage = 0, Data=values });
            values[0] = xResult.ReturnValue;
            
            var yResult = _yExpression.Invoke(runtime, context);
            if (yResult.State == InvocationState.Incomplete)
                return new InvocationResult(new IncompleteInvocationResult(context, yResult.IncompleteResult) { Stage = 1, Data = values });
            values[1] = yResult.ReturnValue;
            
            var zResult = _zExpression.Invoke(runtime, context);
            if (zResult.State == InvocationState.Incomplete)
                return new InvocationResult(new IncompleteInvocationResult(context, zResult.IncompleteResult) { Stage = 2, Data = values });
            var z = zResult.ReturnValue;
            
            return new InvocationResult(InvocationState.None,
                new EarleVector((float)values[0].CastTo(EarleValueType.Float).Value,
                    (float)values[1].CastTo(EarleValueType.Float).Value, (float) z.CastTo(EarleValueType.Float).Value));
        }

        public override InvocationResult Continue(Runtime runtime, IncompleteInvocationResult incompleteInvocationResult)
        {
            var values = incompleteInvocationResult.Data;

            if (incompleteInvocationResult.Stage == 0)
            {
                var xResult = _xExpression.Continue(runtime, incompleteInvocationResult.InnerResult);
                if (xResult.State == InvocationState.Incomplete)
                    return new InvocationResult(new IncompleteInvocationResult(incompleteInvocationResult.Context, xResult.IncompleteResult) { Stage = 0, Data = values });
                values[0] = xResult.ReturnValue;
            }

            if (incompleteInvocationResult.Stage <= 1)
            {
                var yResult = incompleteInvocationResult.Stage == 0
                    ? _yExpression.Invoke(runtime, incompleteInvocationResult.Context)
                    : _yExpression.Continue(runtime, incompleteInvocationResult.InnerResult);

                if (yResult.State == InvocationState.Incomplete)
                    return new InvocationResult(new IncompleteInvocationResult(incompleteInvocationResult.Context, yResult.IncompleteResult) { Stage = 1, Data = values });
                values[1] = yResult.ReturnValue;
            }

            var zResult = incompleteInvocationResult.Stage == 2
                ? _zExpression.Continue(runtime, incompleteInvocationResult.InnerResult)
                : _zExpression.Invoke(runtime, incompleteInvocationResult.Context);

            if (zResult.State == InvocationState.Incomplete)
                return new InvocationResult(new IncompleteInvocationResult(incompleteInvocationResult.Context, zResult.IncompleteResult) { Stage = 2, Data = values });
            var z = zResult.ReturnValue;

            return new InvocationResult(InvocationState.None,
                new EarleVector((float)values[0].CastTo(EarleValueType.Float).Value,
                    (float)values[1].CastTo(EarleValueType.Float).Value, (float)z.CastTo(EarleValueType.Float).Value));
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