﻿// EarleCode
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

namespace EarleCode.Blocks
{
    public class StatementReturn : Block
    {
        private readonly IExpression _expression;

        public StatementReturn(IScriptScope scriptScope, IExpression expression) : base(scriptScope)
        {
            _expression = expression;
        }

        #region Overrides of Block

        public override InvocationResult Invoke(IEarleContext context)
        {
            // todo : states

            return new InvocationResult(InvocationState.Returned,
                _expression?.Invoke(context).ReturnValue ?? EarleValue.Null);
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