using System;

namespace EarleCode.Blocks
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
            var x = _xExpression.Invoke(runtime, context).ReturnValue;
            var y = _yExpression.Invoke(runtime, context).ReturnValue;
            var z = _zExpression.Invoke(runtime, context).ReturnValue;

            return new InvocationResult(InvocationState.None,
                new EarleVector((float) x.CastTo(EarleValueType.Float).Value,
                    (float) z.CastTo(EarleValueType.Float).Value, (float) z.CastTo(EarleValueType.Float).Value));
        }

        public override InvocationResult Continue(Runtime runtime, IncompleteInvocationResult incompleteInvocationResult)
        {
            throw new NotImplementedException();
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