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

using EarleCode.Blocks.Expressions;
using EarleCode.Values;

namespace EarleCode.Blocks.Statements
{
    public class StatementReturn : Block
    {
        private readonly IExpression _expression;

        public StatementReturn(IScriptScope scriptScope, IExpression expression) : base(scriptScope)
        {
            _expression = expression;
        }

        #region Overrides of Block

        public override InvocationResult Invoke(Runtime runtime, IEarleContext context)
        {
            if (_expression == null)
                return new InvocationResult(InvocationState.Returned, EarleValue.Null);

            var result = _expression.Invoke(runtime, context);
            return result.State == InvocationState.Incomplete
                ? result
                : new InvocationResult(InvocationState.Returned, result.ReturnValue);
        }

        public override InvocationResult Continue(Runtime runtime, IncompleteInvocationResult incompleteInvocationResult)
        {
            if (_expression == null)
                return new InvocationResult(InvocationState.Returned, EarleValue.Null);

            var result = _expression.Continue(runtime, incompleteInvocationResult);
            return result.State == InvocationState.Incomplete
                ? result
                : new InvocationResult(InvocationState.Returned, result.ReturnValue);
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            return _expression == null ? "return;" : $"return {_expression};";
        }

        #endregion
    }
}