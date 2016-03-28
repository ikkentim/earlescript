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

using System.Collections.Generic;
using EarleCode.Values;

namespace EarleCode
{
    public class RuntimeScope : IRuntimeScope
    {
        private readonly Dictionary<string, EarleValue> _locals = new Dictionary<string, EarleValue>();
        private readonly RuntimeScope _superScope;

        public RuntimeScope(RuntimeScope superScope) : this(superScope, null)
        {
        }

        public RuntimeScope(RuntimeScope superScope, IDictionary<string, EarleValue> initialLocals)
        {
            _superScope = superScope;

            if (initialLocals != null)
            {
                foreach (var i in initialLocals)
                    _locals[i.Key] = i.Value;
            }
        }

        public virtual EarleValue? GetValue(EarleVariableReference reference)
        {
            var value = _superScope?.GetValue(reference);

            if (value != null)
                return value;

            if (value == null)
            {
                EarleValue local;
                if (_locals.TryGetValue(reference.Name, out local))
                    return local;
            }

            return null;
        }

        public virtual bool SetValue(EarleVariableReference reference, EarleValue value)
        {
            if (_superScope?.GetValue(reference) != null)
            {
                return _superScope.SetValue(reference, value);
            }

            if (CanAssignReferenceAsLocal(reference))
            {
                _locals[reference.Name] = value;
                return true;
            }

            return false;
        }

        protected virtual bool CanAssignReferenceAsLocal(EarleVariableReference reference)
        {
            return reference.File == null;
        }
    }
}