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
using EarleCode.Values;

namespace EarleCode.Blocks.Expressions
{
    public static class ExpressionUtility
    {
        public static bool Invoke(IExpression expression, int stage, Runtime runtime, IEarleContext context,
            out InvocationResult result)
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression));

            var innerResult = expression.Invoke(runtime, context);

            result = innerResult.State == InvocationState.None
                ? innerResult
                : new InvocationResult(new IncompleteInvocationResult(context, innerResult.IncompleteResult)
                {
                    Stage = stage
                });

            return innerResult.State == InvocationState.None;
        }

        public static bool Invoke(IExpression expression, int stage, Runtime runtime, IEarleContext context,
            out InvocationResult result, ref EarleValue value)
        {
            if (Invoke(expression, stage, runtime, context, out result))
            {
                value = result.ReturnValue;
                return true;
            }

            return false;
        }

        public static bool Invoke<T>(IExpression expression, int stage, Runtime runtime, IEarleContext context,
            out InvocationResult result, ref T value)
        {
            var earleValue = EarleValue.Null;
            if (Invoke(expression, stage, runtime, context, out result, ref earleValue))
            {
                value = earleValue.Cast<T>();
                return true;
            }

            return false;
        }

        public static bool Continue(IExpression expression, int stage, Runtime runtime,
            IncompleteInvocationResult incompleteInvocationResult, ref InvocationResult result)
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression));

            if (incompleteInvocationResult.Stage > stage)
                return true;

            var innerResult = incompleteInvocationResult.Stage == stage
                ? expression.Continue(runtime, incompleteInvocationResult.InnerResult)
                : expression.Invoke(runtime, incompleteInvocationResult.Context);

            result = innerResult.State == InvocationState.None
                ? innerResult
                : new InvocationResult(new IncompleteInvocationResult(incompleteInvocationResult.Context,
                    innerResult.IncompleteResult)
                {Stage = stage});

            return innerResult.State == InvocationState.None;
        }

        public static bool Continue(IExpression expression, int stage, Runtime runtime,
            IncompleteInvocationResult incompleteInvocationResult, ref InvocationResult result, ref EarleValue value)
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression));

            if (incompleteInvocationResult.Stage > stage)
                return true;

            if (Continue(expression, stage, runtime, incompleteInvocationResult, ref result))
            {
                value = result.ReturnValue;
                return true;
            }

            return false;
        }

        public static bool Continue<T>(IExpression expression, int stage, Runtime runtime,
            IncompleteInvocationResult incompleteInvocationResult, ref InvocationResult result, ref T value)
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression));

            if (incompleteInvocationResult.Stage > stage)
                return true;

            var earleValue = EarleValue.Null;
            var output = Continue(expression, stage, runtime, incompleteInvocationResult, ref result, ref earleValue);

            if (output)
                value = earleValue.Cast<T>();
            return output;
        }
    }
}