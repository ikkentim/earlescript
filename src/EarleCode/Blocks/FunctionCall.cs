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

        #region Overrides of Block

        public override InvocationResult Invoke(IEarleContext context)
        {
            Debug.WriteLine("Invoking function call " + _functionSignature);
            return Invoke(context, _arguments);
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
            return $"CALL {_functionSignature}({string.Join(", ", (object[])_arguments)})";
        }

        #endregion

        public InvocationResult Invoke(IEarleContext context, params IExpression[] arguments)
        {
            var function = ScriptScope.ResolveFunction(_functionSignature);

            if (function == null)
                throw new CodeException($"Function `{_functionSignature}` not found");

            // TODO: States...
            return function.Invoke(context, arguments.Select(a => a.Invoke(context).ReturnValue).ToArray());
        }
        
    }
}