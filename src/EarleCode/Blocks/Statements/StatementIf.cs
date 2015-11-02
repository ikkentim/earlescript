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

namespace EarleCode.Blocks.Statements
{
    public class StatementIf : ScopeBlock
    {
        private readonly IExpression _expression;

        public StatementIf(IScriptScope scriptScope, IExpression expression) : base(scriptScope)
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression));
            _expression = expression;
        }

        #region Overrides of Block

        public override InvocationResult Invoke(Runtime runtime, IEarleContext context)
        {
            Variables.Clear();

            {
                var result = _expression.Invoke(runtime, context);

                if(result.State == InvocationState.Incomplete)
                    return new InvocationResult(new IncompleteInvocationResult(context, result.IncompleteResult) { Stage = 0, Variables = Variables});

                if (!result.ReturnValue.ToBoolean())
                    return InvocationResult.Empty;
            }

            {
                var result = InvokeBlocks(runtime, context);

                return result.State == InvocationState.Incomplete
                    ? new InvocationResult(new IncompleteInvocationResult(context, result.IncompleteResult) { Stage = 1, Variables = Variables })
                    : result;
            }
        }

        public override InvocationResult Continue(Runtime runtime, IncompleteInvocationResult incompleteInvocationResult)
        {
            Variables = incompleteInvocationResult.Variables;

            if (incompleteInvocationResult.Stage == 0)
            {
                var result = _expression.Continue(runtime, incompleteInvocationResult.InnerResult);

                if (result.State == InvocationState.Incomplete)
                    return new InvocationResult(new IncompleteInvocationResult(incompleteInvocationResult.Context, result.IncompleteResult) { Stage = 0, Variables = Variables });

                if (!result.ReturnValue.ToBoolean())
                    return InvocationResult.Empty;
            }

            {
                var result = incompleteInvocationResult.Stage == 0
                    ? InvokeBlocks(runtime, incompleteInvocationResult.Context)
                    : ContinueBlocks(runtime, incompleteInvocationResult.InnerResult);

                return result.State == InvocationState.Incomplete
                    ? new InvocationResult(new IncompleteInvocationResult(incompleteInvocationResult.Context, result.IncompleteResult) { Stage = 1, Variables = Variables })
                    : result;
            }
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            return $"if({_expression}) {{\n{base.ToString()}\n}}";
        }

        #endregion
    }
}