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

namespace EarleCode.Functions
{
    public class EarleFunction : Block, IEarleFunction
    {
        private readonly VariablesTable _variables = new VariablesTable();

        public EarleFunction(IScriptScope scriptScope, string name, string[] parameterNames) : base(scriptScope)
        {
            Name = name;
            ParameterNames = parameterNames;
        }

        public string Name { get; }
        public string[] ParameterNames { get; }

        public virtual InvocationResult Invoke(IEarleContext context, EarleValue[] args)
        {
            if (args == null) throw new ArgumentNullException(nameof(args));
            if (ParameterNames.Length != args.Length)
                throw new ArgumentException("Invalid argument count", nameof(args));

            _variables.Clear();
            _variables.AddRange(ParameterNames.Zip(args.Select(v => (IVariable) new Variable(v)),
                (p, a) => new KeyValuePair<string, IVariable>(p, a)));


            return Invoke(context);
        }

        #region Overrides of Block

        public override InvocationResult Invoke(IEarleContext context)
        {
            foreach (var block in Blocks)
            {
                var result = block.Invoke(context);

                if (result.State != InvocationState.None)
                    return result;
            }

            return InvocationResult.Empty;
        }

        #region Overrides of Block
        
        public override IVariable AddVariable(string variableName)
        {
            var variable = new Variable();
            _variables.Add(variableName, variable);
            return variable;
        }
        
        public override IVariable ResolveVariable(string variableName)
        {
            return base.ResolveVariable(variableName) ?? _variables.Resolve(variableName);
        }

        #endregion

        #endregion
    }
}