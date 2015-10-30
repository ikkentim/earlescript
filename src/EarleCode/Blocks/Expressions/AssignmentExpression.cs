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
    public class AssignmentExpression : Block, IExpression
    {
        private readonly VariableNameExpression _variableNameExpression;
        private readonly IExpression _expression;

        public AssignmentExpression(IScriptScope scriptScope, VariableNameExpression variableNameExpression,
            IExpression expression) : base(scriptScope)
        {
            if (variableNameExpression == null) throw new ArgumentNullException(nameof(variableNameExpression));
            if (expression == null) throw new ArgumentNullException(nameof(expression));
            _variableNameExpression = variableNameExpression;
            _expression = expression;
        }

        #region Overrides of Block

        public override InvocationResult Invoke(Runtime runtime, IEarleContext context)
        {
            InvocationResult result;
            IVariable variable = null;

            if (!ExpressionUtility.Invoke(_variableNameExpression, 0, runtime, context, out result, ref variable))
                return result;

            if (variable == null)
            {
                if (_variableNameExpression.Indexers.Length == 0)
                {
                    variable = AddVariable(_variableNameExpression.Name);
                }
                else
                {
                    // TODO find out if only last indexer is missing
                    throw new NotImplementedException();
                }
            }

            var setValue = EarleValue.Null;
            if (!ExpressionUtility.Invoke(_expression, 1, runtime, context, out result, ref setValue))
            {
                result.IncompleteResult.Data = new[] {new EarleValue(variable)};
                return result;
            }

            variable.Set(setValue);

            return new InvocationResult(InvocationState.None, setValue);
        }

        public override InvocationResult Continue(Runtime runtime, IncompleteInvocationResult incompleteInvocationResult)
        {
            var result = InvocationResult.Empty;
            var variable = result.IncompleteResult.Data?[0].Cast<IVariable>();

            if (
                !ExpressionUtility.Continue(_variableNameExpression, 0, runtime, incompleteInvocationResult, ref result,
                    ref variable))
                return result;

            if (variable == null)
            {
                if (_variableNameExpression.Indexers.Length == 0)
                {
                    variable = AddVariable(_variableNameExpression.Name);
                }
                else
                {
                    // TODO find out if only last indexer is missing
                    throw new NotImplementedException();
                }
            }

            var setValue = EarleValue.Null;
            if (
                !ExpressionUtility.Continue(_expression, 1, runtime, incompleteInvocationResult, ref result,
                    ref setValue))
            {
                result.IncompleteResult.Data = new[] {new EarleValue(variable)};
                return result;
            }
            variable.Set(setValue);

            return new InvocationResult(InvocationState.None, setValue);
        }

        #endregion

        #region Overrides of Object

        /// <summary>
        ///     Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        ///     A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            return $"{_variableNameExpression} = {_expression};";
        }

        #endregion

    }
}