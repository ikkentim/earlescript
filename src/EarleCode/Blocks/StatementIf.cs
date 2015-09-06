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
    public class StatementIf : Block
    {
        private readonly IExpression _expression;
        
        public StatementIf(IScriptScope scriptScope, IExpression expression) : base(scriptScope)
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression));
            _expression = expression;
        }

        #region Overrides of Block

        public override InvocationResult Invoke(IEarleContext context)
        {
            if (!_expression.Invoke(context).ReturnValue.ToBoolean())
                return InvocationResult.Empty;

            foreach (var block in Blocks)
            {
                var result = block.Invoke(context);

                if (result.State != InvocationState.None)
                    return result;
            }

            return InvocationResult.Empty;
        }

        #endregion
    }
}