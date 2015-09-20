// EarleCode
// Copyright 2015 Tim Potze
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using EarleCode.Blocks;
using EarleCode.Blocks.Expressions;
using EarleCode.Values;

namespace EarleCode.Operators
{
    public abstract class EarleBinaryOperator : IEarleBinaryOperator
    {
        #region Implementation of IEarleBinaryOperator

        public InvocationResult Invoke(Runtime runtime, IEarleContext context, IExpression expression1,
            IExpression expression2)
        {
            var result1 = expression1.Invoke(runtime, context);
            if (result1.State == InvocationState.Incomplete)
                return
                    new InvocationResult(new OperatorIncompleteInvocationResult(context, result1.Result, expression1,
                        expression2, 0));

            if (!IsValue1Acceptable(result1.ReturnValue))
                return new InvocationResult(InvocationState.None, Compute(null, null));

            var result2 = expression2.Invoke(runtime, context);
            if (result2.State == InvocationState.Incomplete)
                return
                    new InvocationResult(new OperatorIncompleteInvocationResult(context, result2.Result, expression1,
                        expression2, result1.ReturnValue, 1));

            return new InvocationResult(InvocationState.None,
                Compute(result1.ReturnValue,
                    IsValue2Acceptable(result2.ReturnValue) ? (EarleValue?) result2.ReturnValue : null));
        }

        public InvocationResult Continue(Runtime runtime, IncompleteInvocationResult incompleteInvocationResult)
        {
            var incomplete = incompleteInvocationResult as OperatorIncompleteInvocationResult;

            var value1 = incomplete.Value1;

            if (incomplete.Stage == 0)
            {
                var result1 = incomplete.Expression1.Continue(runtime, incomplete.InnerResult);
                if (result1.State == InvocationState.Incomplete)
                    return
                        new InvocationResult(new OperatorIncompleteInvocationResult(incomplete.Context, result1.Result,
                            incomplete.Expression1, incomplete.Expression2, 0));

                if (!IsValue1Acceptable(result1.ReturnValue))
                    return new InvocationResult(InvocationState.None, Compute(null, null));

                value1 = result1.ReturnValue;
            }

            var result2 = incomplete.Stage == 1
                ? incomplete.Expression2.Continue(runtime, incomplete.InnerResult)
                : incomplete.Expression2.Invoke(runtime, incomplete.Context);

            return result2.State == InvocationState.Incomplete
                ? new InvocationResult(new OperatorIncompleteInvocationResult(incomplete.Context, result2.Result,
                    incomplete.Expression1, incomplete.Expression2, value1, 1))
                : new InvocationResult(InvocationState.None,
                    Compute(value1, IsValue2Acceptable(result2.ReturnValue) ? (EarleValue?) result2.ReturnValue : null));
        }

        #endregion

        protected virtual bool IsValue1Acceptable(EarleValue value)
        {
            return true;
        }

        protected virtual bool IsValue2Acceptable(EarleValue value)
        {
            return true;
        }

        protected abstract EarleValue Compute(EarleValue? value1, EarleValue? value2);

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