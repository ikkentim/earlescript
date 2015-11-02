using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using EarleCode.Blocks.Expressions;
using EarleCode.Values;

namespace EarleCode.Operators
{
    public class EarleBinaryOperator : IEarleBinaryOperator
    {
        private readonly Func<Func<EarleValue>, Func<EarleValue>, EarleValue> _func;

        public EarleBinaryOperator(Func<Func<EarleValue>,Func<EarleValue>, EarleValue> func)
        {
            if (func == null) throw new ArgumentNullException(nameof(func));
            _func = func;
        }

        #region Implementation of IEarleBinaryOperator

        public InvocationResult Invoke(Runtime runtime, IEarleContext context, IExpression expression1, IExpression expression2)
        {
            EarleValue? value1 = null, value2 = null;

            Func<EarleValue> resolve1 = () =>
            {
                if (value1 != null)
                    return value1.Value;

                var result = expression1.Invoke(runtime, context);
                if (result.State == InvocationState.Incomplete)
                    throw new IncompleteExpressionResolveException(result.IncompleteResult, 0);

                value1 = result.ReturnValue;
                return result.ReturnValue;
            };
            Func<EarleValue> resolve2 = () =>
            {
                if (value2 != null)
                    return value2.Value;

                var result = expression2.Invoke(runtime, context);
                if (result.State == InvocationState.Incomplete)
                    throw new IncompleteExpressionResolveException(result.IncompleteResult, 1);

                value2 = result.ReturnValue;
                return result.ReturnValue;
            };

            try
            {
                return new InvocationResult(InvocationState.None, _func(resolve1, resolve2));
            }
            catch (IncompleteExpressionResolveException e)
            {
                return
                    new InvocationResult(new OperatorIncompleteInvocationResult(context, e.IncompleteInvocationResult,
                        expression1, expression2, value1 ?? EarleValue.Null, e.Stage));
            }
        }

        public InvocationResult Continue(Runtime runtime, IncompleteInvocationResult incompleteInvocationResult)
        {
            var r = incompleteInvocationResult as OperatorIncompleteInvocationResult;

            if(r == null)
                throw new Exception("invalid type of IncompleteInvocationResult");

            var expression1 = r.Expression1;
            var expression2 = r.Expression2;

            EarleValue? value1 = r.Stage == 0 ? null : new EarleValue?(r.Value1), value2 = null;

            Func<EarleValue> resolve1 = () =>
            {
                if (value1 != null)
                    return value1.Value;

                var result = r.Stage == 0
                    ? expression1.Continue(runtime, r.InnerResult)
                    : expression1.Invoke(runtime, r.Context);

                if (result.State == InvocationState.Incomplete)
                    throw new IncompleteExpressionResolveException(result.IncompleteResult, 0);

                value1 = result.ReturnValue;
                return result.ReturnValue;
            };
            Func<EarleValue> resolve2 = () =>
            {
                if (value2 != null)
                    return value2.Value;

                var result = r.Stage == 1
                    ? expression2.Continue(runtime, r.InnerResult)
                    : expression2.Invoke(runtime, r.Context);
                
                if (result.State == InvocationState.Incomplete)
                    throw new IncompleteExpressionResolveException(result.IncompleteResult, 1);

                value2 = result.ReturnValue;
                return result.ReturnValue;
            };

            try
            {
                return new InvocationResult(InvocationState.None, _func(resolve1, resolve2));
            }
            catch (IncompleteExpressionResolveException e)
            {
                return
                    new InvocationResult(new OperatorIncompleteInvocationResult(incompleteInvocationResult.Context, e.IncompleteInvocationResult,
                        expression1, expression2, value1 ?? EarleValue.Null, e.Stage));
            }
        }

        #endregion

        private class IncompleteExpressionResolveException : Exception
        {
            public IncompleteExpressionResolveException(IncompleteInvocationResult incompleteInvocationResult, int stage)
            {
                IncompleteInvocationResult = incompleteInvocationResult;
                Stage = stage;
            }

            public IncompleteInvocationResult IncompleteInvocationResult { get; }
            public int Stage { get; }
        }

        private class OperatorIncompleteInvocationResult : IncompleteInvocationResult
        {
            public OperatorIncompleteInvocationResult(IEarleContext context, IncompleteInvocationResult innerResult,
                IExpression expression1, IExpression expression2, int stage)
                : base(context, innerResult)
            {
                Expression1 = expression1;
                Expression2 = expression2;
                Stage = stage;
            }

            public OperatorIncompleteInvocationResult(IEarleContext context, IncompleteInvocationResult innerResult,
                IExpression expression1, IExpression expression2, EarleValue value1, int stage)
                : base(context, innerResult)
            {
                Expression1 = expression1;
                Expression2 = expression2;
                Value1 = value1;
                Stage = stage;
            }

            public IExpression Expression1 { get; }
            public EarleValue Value1 { get; set; }
            public IExpression Expression2 { get; }
        }
    }


}
