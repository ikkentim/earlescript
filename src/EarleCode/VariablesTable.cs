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

namespace EarleCode
{
    public class VariablesTable : Dictionary<string, IVariable>
    {
        public void Add(KeyValuePair<string, IVariable> keyValuePair)
        {
            Add(keyValuePair.Key, keyValuePair.Value);
        }

        public void AddRange(IEnumerable<KeyValuePair<string, IVariable>> keyValuePairs)
        {
            if (keyValuePairs == null) throw new ArgumentNullException(nameof(keyValuePairs));

            foreach (var keyValuePair in keyValuePairs)
                Add(keyValuePair);
        }

        public IVariable Resolve(string name)
        {
            IVariable variable;
            TryGetValue(name, out variable);
            return variable;
        }
    }
}