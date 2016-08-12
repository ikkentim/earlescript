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

namespace EarleCode.Runtime
{
    internal class EarleFunctionTable : IEnumerable<EarleFunction>
    {
        private readonly Dictionary<string, EarleFunctionCollection> _values =
            new Dictionary<string, EarleFunctionCollection>();

        public void Add(EarleFunction function)
        {
            if (function == null)
                throw new ArgumentNullException(nameof(function));

            EarleFunctionCollection collection;

            if (_values.TryGetValue(function.Name, out collection))
            {
                collection.Add(function);
            }
            else
            {
                collection = new EarleFunctionCollection();
                collection.Add(function);
                _values.Add(function.Name, collection);
            }
        }

        public EarleFunctionCollection Get(string functionName)
        {
            if (functionName == null)
                throw new ArgumentNullException(nameof(functionName));
            EarleFunctionCollection collection;

            _values.TryGetValue(functionName, out collection);
            return collection;
        }

        #region Implementation of IEnumerable<EarleFunction>

        public IEnumerator<EarleFunction> GetEnumerator()
        {
            return _values.Values.SelectMany(c => c).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}