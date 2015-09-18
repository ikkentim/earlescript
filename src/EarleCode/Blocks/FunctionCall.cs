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
using System.Diagnostics;
using System.Linq;
using EarleCode.Functions;
using EarleCode.Parsers;

namespace EarleCode.Blocks
{
    public class FunctionCall : Block, IExpression
    {
        private readonly IExpression[] _arguments;
        private readonly EarleFunctionSignature _functionSignature;

        public FunctionCall(IScriptScope scriptScope, EarleFunctionSignature functionSignature,
            params IExpression[] arguments)
            : base(scriptScope)
        {
            if (arguments == null) throw new ArgumentNullException(nameof(arguments));
            _functionSignature = functionSignature;
            _arguments = arguments;
        }
        
        private IEarleFunction GetFunction()
        {
            var function = ScriptScope.ResolveFunction(_functionSignature);

            if (function == null)
                throw new Exception($"Function `{_functionSignature}` not found");

            return function;
        }

        #region Overrides of Block

        public override InvocationResult Invoke(IEarleContext context)
        {
            var parameters = new EarleValue[_arguments.Length];
            for (var i = 0; i < _arguments.Length; i++)
            {
                var arg = _arguments[i];

                var result = arg.Invoke(context);

                if (result.State == InvocationState.Incomplete)
                {
                    return new InvocationResult(new IncompleteInvocationResult(context, result.Result, i, parameters));
                }

                parameters[i] = result.ReturnValue;
            }

            var functionResult = GetFunction().Invoke(context, parameters);

            if (functionResult.State == InvocationState.Incomplete)
            {
                return new InvocationResult(new IncompleteInvocationResult(context, functionResult.Result, -1, null));
            }
            
            return new InvocationResult(InvocationState.None, functionResult.ReturnValue);
        }

        public override InvocationResult Continue(IncompleteInvocationResult incompleteInvocationResult)
        {
            var parameters = incompleteInvocationResult.Data;


            if (incompleteInvocationResult.Stage != -1)
            {
                for (var i = incompleteInvocationResult.Stage; i < _arguments.Length; i++)
                {
                    var arg = _arguments[i];

                    var result = i == incompleteInvocationResult.Stage
                        ? arg.Continue(incompleteInvocationResult.InnerResult)
                        : arg.Invoke(incompleteInvocationResult.Context);

                    if (result.State == InvocationState.Incomplete)
                    {
                        return
                            new InvocationResult(new IncompleteInvocationResult(incompleteInvocationResult.Context,
                                result.Result, i, parameters));
                    }

                    parameters[i] = result.ReturnValue;
                }

                var functionResult = GetFunction().Invoke(incompleteInvocationResult.Context, parameters);

                if (functionResult.State == InvocationState.Incomplete)
                {
                    return new InvocationResult(new IncompleteInvocationResult(incompleteInvocationResult.Context, functionResult.Result, -1, null));
                }

                return new InvocationResult(InvocationState.None, functionResult.ReturnValue);
            }
            else
            {
                var functionResult = GetFunction().Continue(incompleteInvocationResult.InnerResult);

                if (functionResult.State == InvocationState.Incomplete)
                {
                    return new InvocationResult(new IncompleteInvocationResult(incompleteInvocationResult.Context, functionResult.Result, -1, null));
                }

                return new InvocationResult(InvocationState.None, functionResult.ReturnValue);
            }

        }

        #endregion

        #region Overrides of Object

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"{_functionSignature}({string.Join(", ", (object[])_arguments)});";
        }

        #endregion
    }
}