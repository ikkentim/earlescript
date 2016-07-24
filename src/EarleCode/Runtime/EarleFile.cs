// EarleCode
// Copyright 2016 Tim Potze
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
using EarleCode.Runtime.Values;

namespace EarleCode.Runtime
{
    public class EarleFile : EarleRuntimeScope
    {
        private readonly EarleRuntime _runtime;

        public EarleFile(EarleRuntime runtime, string name) : base(runtime)
        {
            if (runtime == null) throw new ArgumentNullException(nameof(runtime));
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (!IsValidName(name)) throw new ArgumentException("invalid name", nameof(name));

            _runtime = runtime;
            Name = name;
        }

        public string Name { get; }

        public List<EarleFunction> Functions { get; } = new List<EarleFunction>();

        public static bool IsValidName(string input)
        {
            // todo improve
            return input.StartsWith("\\");
        }

        public void AddFunction(EarleFunction function)
        {
            if (function == null) throw new ArgumentNullException(nameof(function));
            Functions.Add(function);
        }

        public EarleFunctionCollection GetFunctions(string name)
        {
            var funcs = Functions.Where(f => f.Name == name).ToArray();
            return funcs.Length == 0 ? null : new EarleFunctionCollection(funcs);
        }

        public EarleValue? Invoke(string functionName, EarleValue target, params EarleValue[] arguments)
        {
            var function = GetFunctions(functionName).FirstOrDefault(f => f.Parameters.Length == arguments.Length);
            return _runtime.Invoke(function, arguments, target);
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
            return Name;
        }

        #endregion

        #region Overrides of RuntimeScope

        public override EarleValue GetValue(EarleVariableReference reference)
        {
            var baseResult = base.GetValue(reference);

            if (!baseResult.HasValue || baseResult.Is<EarleFunctionCollection>())
                if (reference.File == Name || reference.File == null)
                {
                    var functions = GetFunctions(reference.Name);

                    if (functions != null)
                    {
                        if (baseResult.HasValue && baseResult.Is<EarleFunctionCollection>())
                            functions.AddRange(baseResult.As<EarleFunctionCollection>());

                        return new EarleValue(functions);
                    }
                }

            return baseResult;
        }

        protected override bool CanAssignReferenceAsLocal(EarleVariableReference reference)
        {
            return false;
        }

        #endregion
    }
}