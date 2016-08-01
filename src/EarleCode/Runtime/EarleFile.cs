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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using EarleCode.Runtime.Values;

namespace EarleCode.Runtime
{
    public class EarleFile : EarleRuntimeScope, IEnumerable<EarleFunction>
    {
        private EarleFunctionTable _functions = new EarleFunctionTable();
        private readonly List<string> _includedFiles = new List<string>();

        public EarleFile(EarleRuntime runtime, string name) : base(runtime)
        {
            if (runtime == null) throw new ArgumentNullException(nameof(runtime));
            if (name == null) throw new ArgumentNullException(nameof(name));

            Runtime = runtime;
            Name = name;
        }

        public string Name { get; }

        public EarleRuntime Runtime { get; }

        public EarleFunctionCollection this[string functionName]
        {
            get { return GetFunctions(functionName); }
        }

        public void IncludeFile(string file)
        {
            if(!_includedFiles.Contains(file))
                _includedFiles.Add(file);
        }

        public void AddFunction(EarleFunction function)
        {
            if (function == null) throw new ArgumentNullException(nameof(function));

            _functions.Add(function);
        }

        public EarleFunctionCollection GetFunctions(string functionName)
        {
            return _functions.Get(functionName);
        }

        public EarleValue? Invoke(string functionName, EarleCompletionHandler completionHandler, EarleValue target, params EarleValue[] arguments)
        {
            return GetFunctions(functionName).Invoke(completionHandler, target, arguments);
        }

        #region Overrides of RuntimeScope

        public override EarleValue GetValue(EarleVariableReference reference)
        {
            var baseResult = base.GetValue(reference);

            if (!baseResult.HasValue || baseResult.Is<EarleFunctionCollection>())
                if (reference.File == Name || reference.File == null)
                {
                    var functions = GetFunctions(reference.Name);

                    if(reference.File == null)
                    {
                        foreach(var inc in _includedFiles)
                        {
                            var incVal = Runtime.GetValue(new EarleVariableReference(inc, reference.Name));
                            if(incVal.Is<EarleFunctionCollection>())
                            {
                                if(functions == null)
                                    functions = new EarleFunctionCollection();

                                functions.AddRange(incVal.As<EarleFunctionCollection>());
                            }
                        }
                    }

                    if (functions != null)
                    {
                        if (baseResult.HasValue && baseResult.Is<EarleFunctionCollection>())
                            functions.AddRange(baseResult.As<EarleFunctionCollection>());

                        return new EarleValue(functions);
                    }

                }

            return baseResult;
        }

        protected override bool CanAssignReferenceInScope(EarleVariableReference reference)
        {
            return false;
        }

        #endregion

        #region Implementation of IEnumerable<EarleFunction>

        public IEnumerator<EarleFunction> GetEnumerator()
        {
            return _functions.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}