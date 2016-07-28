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
        private readonly EarleDictionary _locals = new EarleDictionary();
        private readonly IEarleRuntimeScope _superScope;

        public  EarleRuntimeScope(IEarleRuntimeScope superScope) : this(superScope, null)
        {
        }

        public EarleRuntimeScope(IEarleRuntimeScope superScope, EarleDictionary initialLocals)
        {
            _superScope = superScope;

            if (initialLocals != null)
            {
                foreach (var i in initialLocals)
                    _locals[i.Key] = i.Value;
            }
        }

        public virtual EarleValue GetValue(EarleVariableReference reference)
        {
            var value = _superScope?.GetValue(reference);

            if (value?.HasValue ?? false)
                return value.Value;

            return _locals[reference.Name];
        }

        public virtual bool SetValue(EarleVariableReference reference, EarleValue value)
        {
            if ((_superScope?.GetValue(reference) ?? EarleValue.Undefined).HasValue)
            {
                return _superScope.SetValue(reference, value);
            }

            if (CanAssignReferenceInScope(reference))
            {
                _locals[reference.Name] = value;
                return true;
            }

            return false;
        }

        protected virtual bool CanAssignReferenceInScope(EarleVariableReference reference)
        {
            return reference.File == null;
        }
    }
}