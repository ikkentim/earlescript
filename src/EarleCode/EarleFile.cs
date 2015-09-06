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
using EarleCode.Functions;

namespace EarleCode
{
    public class EarleFile : IScriptScope
    {
        private readonly Runtime _runtime;

        public EarleFile(Runtime runtime, string path)
        {
            if (runtime == null) throw new ArgumentNullException(nameof(runtime));
            if (path == null) throw new ArgumentNullException(nameof(path));

            _runtime = runtime;
            Path = path;
        }

        protected Dictionary<string, IEarleFunction> Functions { get; } = new Dictionary<string, IEarleFunction>();
        public string Path { get; }

        public virtual void AddFunction(string functionName, IEarleFunction function)
        {
            Functions.Add(functionName, function);
        }

        #region Implementation of IScriptScope

        public IVariable ResolveVariable(string variableName)
        {
            return _runtime.ResolveVariable(variableName);
        }

        public IEarleFunction ResolveFunction(EarleFunctionSignature functionSignature)
        {
            if (functionSignature.Path == null)
            {
                IEarleFunction function;
                Functions.TryGetValue(functionSignature.Name, out function);
                return function ?? _runtime.ResolveFunction(functionSignature);
            }
            else
            {
                if (Path != functionSignature.Path)
                    return _runtime.ResolveFunction(functionSignature);

                IEarleFunction function;
                Functions.TryGetValue(functionSignature.Name, out function);
                return function;
            }
        }

        public IVariable AddVariable(string variableName)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}