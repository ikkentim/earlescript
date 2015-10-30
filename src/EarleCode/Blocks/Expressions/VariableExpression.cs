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
using System.Linq;
using EarleCode.Values;

namespace EarleCode.Blocks.Expressions
{
    public class VariableExpression : Block, IExpression
    {
        private readonly VariableNameExpression _variableNameExpression;

        public VariableExpression(IScriptScope scriptScope, VariableNameExpression variableNameExpression)
            : base(scriptScope)
        {
            if (variableNameExpression == null) throw new ArgumentNullException(nameof(variableNameExpression));
            _variableNameExpression = variableNameExpression;
        }

        #region Overrides of Block

        public override InvocationResult Invoke(Runtime runtime, IEarleContext context)
        {
            InvocationResult result;
            IVariable variable = null;

            return !ExpressionUtility.Invoke(_variableNameExpression, 0, runtime, context, out result, ref variable)
                ? result
                : new InvocationResult(InvocationState.None, variable?.Get() ?? EarleValue.Null);
        }

        public override InvocationResult Continue(Runtime runtime, IncompleteInvocationResult incompleteInvocationResult)
        {
            var result = InvocationResult.Empty;
            IVariable variable = null;

            return
                !ExpressionUtility.Continue(_variableNameExpression, 0, runtime, incompleteInvocationResult, ref result,
                    ref variable)
                    ? result
                    : new InvocationResult(InvocationState.None, variable?.Get() ?? EarleValue.Null);
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
            return _variableNameExpression.ToString();
        }

        #endregion
    }
}