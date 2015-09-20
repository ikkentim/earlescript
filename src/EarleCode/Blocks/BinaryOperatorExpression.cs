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
    public class BinaryOperatorExpression : Block, IExpression
    {
        public IExpression LeftExpression { get; }
        public string OperatorToken { get; }
        public IExpression RightExpression { get; }

        public BinaryOperatorExpression(IScriptScope scriptScope, IExpression leftExpression, string operatorToken,
            IExpression rightExpression) : base(scriptScope)
        {
            if (leftExpression == null) throw new ArgumentNullException(nameof(leftExpression));
            if (operatorToken == null) throw new ArgumentNullException(nameof(operatorToken));
            if (rightExpression == null) throw new ArgumentNullException(nameof(rightExpression));
            LeftExpression = leftExpression;
            OperatorToken = operatorToken;
            RightExpression = rightExpression;
        }

        #region Overrides of Block

        public override InvocationResult Invoke(Runtime runtime, IEarleContext context)
        {
            return runtime.GetOperator(OperatorToken)?.Invoke(runtime, context, LeftExpression, RightExpression) ??
                   InvocationResult.Empty;
        }

        public override InvocationResult Continue(Runtime runtime, IncompleteInvocationResult incompleteInvocationResult)
        {
            return runtime.GetOperator(OperatorToken)?.Continue(runtime, incompleteInvocationResult) ??
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
            return $"{LeftExpression} {OperatorToken} {RightExpression}";
        }

        #endregion
    }
}