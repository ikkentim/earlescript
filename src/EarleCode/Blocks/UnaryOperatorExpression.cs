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

namespace EarleCode.Blocks
{
    public class UnaryOperatorExpression : Block, IExpression
    {
        public UnaryOperatorExpression(IScriptScope scriptScope, string operatorToken,
            IExpression expression) : base(scriptScope)
        {
            if (operatorToken == null) throw new ArgumentNullException(nameof(operatorToken));
            if (expression == null) throw new ArgumentNullException(nameof(expression));
            OperatorToken = operatorToken;
            Expression = expression;
        }

        public string OperatorToken { get; }
        public IExpression Expression { get; }

        #region Overrides of Block

        public override InvocationResult Invoke(Runtime runtime, IEarleContext context)
        {
            return runtime.GetUnaryOperator(OperatorToken)?.Invoke(runtime, context, Expression) ??
                   InvocationResult.Empty;
        }

        public override InvocationResult Continue(Runtime runtime, IncompleteInvocationResult incompleteInvocationResult)
        {
            return runtime.GetUnaryOperator(OperatorToken)?.Continue(runtime, incompleteInvocationResult) ??
                   InvocationResult.Empty;
        }

        #endregion

        #region Overrides of Object

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            return $"{OperatorToken}{Expression}";
        }

        #endregion
    }
}