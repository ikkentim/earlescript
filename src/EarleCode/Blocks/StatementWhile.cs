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
    public class StatementWhile : Block
    {
        private readonly IExpression _expression;

        public StatementWhile(IScriptScope scriptScope, IExpression expression) : base(scriptScope)
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression));
            _expression = expression;
        }

        #region Overrides of Block

        public override InvocationResult Invoke(IEarleContext context)
        {
            while (_expression.Invoke(context).ReturnValue.ToBoolean())
            {
                foreach (var block in Blocks)
                {
                    var result = block.Invoke(context);

                    if (result.State != InvocationState.None)
                        return result;
                }
            }

            return InvocationResult.Empty;
        }

        /// <summary>
        ///     Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        ///     A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            return $"while({_expression}) {{\n{base.ToString()}\n}}";
        }

        #endregion
    }
}