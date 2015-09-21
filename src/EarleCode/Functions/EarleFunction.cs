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
using System.Collections.Generic;
using System.Linq;
using EarleCode.Blocks;
using EarleCode.Values;

namespace EarleCode.Functions
{
    public class EarleFunction : ScopeBlock, IEarleFunction
    {
        public EarleFunction(IScriptScope scriptScope, string name, string[] parameterNames) : base(scriptScope)
        {
            Name = name;
            ParameterNames = parameterNames;
        }

        public string Name { get; }
        public string[] ParameterNames { get; }

        public virtual InvocationResult Invoke(Runtime runtime, IEarleContext context, EarleValue[] args)
        {
            if (args == null) throw new ArgumentNullException(nameof(args));
            if (ParameterNames.Length != args.Length)
                throw new ArgumentException("Invalid argument count", nameof(args));

            Variables.Clear();
            Variables.AddRange(ParameterNames.Zip(args.Select(v => (IVariable) new Variable(v)),
                (p, a) => new KeyValuePair<string, IVariable>(p, a)));

            Variables.Add("self", new Variable(context == null ? EarleValue.Null : new EarleValue(context)));

            return Invoke(runtime, context);
        }

        #region Overrides of Block

        public override InvocationResult Invoke(Runtime runtime, IEarleContext context)
        {
            var result = InvokeBlocks(runtime, context);

            return result.State == InvocationState.Incomplete
                ? new InvocationResult(new IncompleteInvocationResult(context, result.Result) {Variables = Variables.Clone()})
                : result;
        }

        public override InvocationResult Continue(Runtime runtime, IncompleteInvocationResult incompleteInvocationResult)
        {
            Variables = incompleteInvocationResult.Variables;

            var result = ContinueBlocks(runtime, incompleteInvocationResult.InnerResult);

            return result.State == InvocationState.Incomplete
                ? new InvocationResult(new IncompleteInvocationResult(incompleteInvocationResult.Context, result.Result)
                {
                    Variables = Variables.Clone()
                })
                : result;
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
            return $"{Name} ({string.Join(", ", ParameterNames)}) {{\n{base.ToString()}\n}}";
        }

        #endregion
    }
}