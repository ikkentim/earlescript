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

using System;
using EarleCode.Blocks.Expressions;
using EarleCode.Values;

namespace EarleCode.Operators
{
    public class EarleUnaryOperator : IEarleUnaryOperator
    {
        private readonly Func<EarleValue, EarleValue> _compute;

        public EarleUnaryOperator(Func<EarleValue, EarleValue> compute)
        {
            if (compute == null) throw new ArgumentNullException(nameof(compute));
            _compute = compute;
        }
        
        #region Implementation of IEarleBinaryOperator

        public InvocationResult Invoke(Runtime runtime, IEarleContext context, IExpression expression)
        {
            var result = expression.Invoke(runtime, context);

            return result.State == InvocationState.Incomplete
                ? new InvocationResult(new OperatorIncompleteInvocationResult(context, result.IncompleteResult, expression))
                : new InvocationResult(InvocationState.None, _compute(result.ReturnValue));
        }

        public InvocationResult Continue(Runtime runtime, IncompleteInvocationResult incompleteInvocationResult)
        {
            var incomplete = incompleteInvocationResult as OperatorIncompleteInvocationResult;

            var result = incomplete.Expression.Continue(runtime, incomplete.InnerResult);

            return result.State == InvocationState.Incomplete
                ? new InvocationResult(new OperatorIncompleteInvocationResult(incomplete.Context, result.IncompleteResult,
                    incomplete.Expression))
                : new InvocationResult(InvocationState.None, _compute(result.ReturnValue));
        }

        #endregion

        private class OperatorIncompleteInvocationResult : IncompleteInvocationResult
        {
            public OperatorIncompleteInvocationResult(IEarleContext context, IncompleteInvocationResult innerResult, IExpression expression)
                : base(context, innerResult)
            {
                Expression = expression;
            }

            public IExpression Expression { get; }
        }
    }
}