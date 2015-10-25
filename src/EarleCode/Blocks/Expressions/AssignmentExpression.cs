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
    public class AssignmentExpression : Block, IExpression
    {
        private readonly IExpression _expression;
        private readonly IExpression[] _indexers;
        private readonly string _name;

        public AssignmentExpression(IScriptScope scriptScope, string name, IExpression[] indexers,
            IExpression expression) : base(scriptScope)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (indexers == null) throw new ArgumentNullException(nameof(indexers));
            if (expression == null) throw new ArgumentNullException(nameof(expression));
            _name = name;
            _indexers = indexers;
            _expression = expression;
        }
        
        private EarleValue SetVariable(string name, EarleValue value)
        {
            var variable = ResolveVariable(name) ?? AddVariable(name);
            variable.Set(value);

            return value;
        }

        #region Overrides of Block

        public override InvocationResult Invoke(Runtime runtime, IEarleContext context)
        {
            var variable = ResolveVariable(_name) ?? AddVariable(_name);
            for (var i = 0; i < _indexers.Length; i++)
            {
                
            }

            var result = _expression.Invoke(runtime, context);

            return result.State == InvocationState.Incomplete
                ? new InvocationResult(new IncompleteInvocationResult("assignment 0", context, result.IncompleteResult)
                {
                    Stage = _indexers.Length
                })
                : new InvocationResult(InvocationState.None, SetVariable(_name, result.ReturnValue));
        }

        public override InvocationResult Continue(Runtime runtime, IncompleteInvocationResult incompleteInvocationResult)
        {
            var result = _expression.Continue(runtime, incompleteInvocationResult.InnerResult);

            return result.State == InvocationState.Incomplete
                ? new InvocationResult(new IncompleteInvocationResult("assignment c0", incompleteInvocationResult.Context, result.IncompleteResult))
                : new InvocationResult(InvocationState.None, SetVariable(_name, result.ReturnValue));
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
            return $"{_name}{string.Concat(_indexers.Select(i => $"[{i}]"))} = {_expression};";
        }

        #endregion

    }
}