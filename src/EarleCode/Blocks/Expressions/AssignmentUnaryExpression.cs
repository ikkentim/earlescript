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
    public class AssignmentUnaryExpression : Block, IExpression
    {
        private readonly bool _isPostOperation;
        private readonly VariableNameExpression _variableNameExpression;
        private readonly string _operatorToken;

        public AssignmentUnaryExpression(IScriptScope scriptScope, VariableNameExpression variableNameExpression,
            string operatorToken, bool isPostOperation) : base(scriptScope)
        {
            if (variableNameExpression == null) throw new ArgumentNullException(nameof(variableNameExpression));
            if (operatorToken == null) throw new ArgumentNullException(nameof(operatorToken));
            _variableNameExpression = variableNameExpression;
            _operatorToken = operatorToken;
            _isPostOperation = isPostOperation;
        }
        
        #region Overrides of Object

        /// <summary>
        ///     Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        ///     A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            return _isPostOperation
                ? $"{_variableNameExpression}{_operatorToken};"
                : $"{_operatorToken}{_variableNameExpression}";
        }

        #endregion

        #region Overrides of Block

        public override InvocationResult Invoke(Runtime runtime, IEarleContext context)
        {
            InvocationResult result;
            IVariable variable = null;

            if (!ExpressionUtility.Invoke(_variableNameExpression, 0, runtime, context, out result, ref variable))
                return result;

            if (variable == null)
            {
                throw new Exception("variable not found");
            }

            var preValue = variable.Get();
            var postValue = new EarleValue(null);

            switch (_operatorToken)
            {
                case "--":
                    switch (preValue.Type)
                    {
                        case EarleValueType.Integer:
                            postValue = (int) preValue.Value - 1;
                            break;
                        case EarleValueType.Float:
                            postValue = (float) preValue.Value - 1;
                            break;
                    }
                    break;
                case "++":
                    switch (preValue.Type)
                    {
                        case EarleValueType.Integer:
                            postValue = (int) preValue.Value + 1;
                            break;
                        case EarleValueType.Float:
                            postValue = (float) preValue.Value + 1;
                            break;
                    }
                    break;
            }

            variable.Set(postValue);

            return new InvocationResult(InvocationState.None, _isPostOperation ? preValue : postValue);
        }

        public override InvocationResult Continue(Runtime runtime, IncompleteInvocationResult incompleteInvocationResult)
        {
            // only incomplete causes may be indexers which are not implemented yet
            throw new NotImplementedException();
        }

        #endregion
    }
}