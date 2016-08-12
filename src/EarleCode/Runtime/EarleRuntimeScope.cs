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

using EarleCode.Runtime.Values;

namespace EarleCode.Runtime
{
    public class EarleRuntimeScope : IEarleRuntimeScope
    {
        private readonly EarleDictionary _locals;
        private readonly IEarleRuntimeScope _superScope;

        public EarleRuntimeScope(IEarleRuntimeScope superScope) : this(superScope, null)
        {
        }

        public EarleRuntimeScope(IEarleRuntimeScope superScope, EarleDictionary locals)
        {
            _superScope = superScope;
            _locals = locals ?? new EarleDictionary();
        }

        public virtual EarleValue GetValue(string name)
        {
            var value = _superScope?.GetValue(name);

            if (value?.HasValue ?? false)
                return value.Value;

            return _locals[name];
        }

        public virtual bool SetValue(string name, EarleValue value)
        {
            if ((_superScope?.GetValue(name) ?? EarleValue.Undefined).HasValue)
            {
                return _superScope.SetValue(name, value);
            }

            if (CanAssignVariableInScope(name))
            {
                _locals[name] = value;
                return true;
            }

            return false;
        }

        public virtual EarleFunctionCollection GetFunctionReference(string fileName, string functionName)
        {
            return _superScope == null ? null : _superScope.GetFunctionReference(fileName, functionName);
        }

        protected virtual bool CanAssignVariableInScope(string name)
        {
            return true;
        }
    }
}