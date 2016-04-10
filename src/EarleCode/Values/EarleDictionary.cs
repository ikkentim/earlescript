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

namespace EarleCode.Values
{
    public class EarleDictionary : IEnumerable<KeyValuePair<string, EarleValue>>
    {
        private readonly Dictionary<string, EarleValue> _values = new Dictionary<string, EarleValue>();

        public EarleValue this[string key]
        {
            get
            {
                if (key == null) throw new ArgumentNullException(nameof(key));
                EarleValue value;
                return _values.TryGetValue(key, out value) ? value : EarleValue.Undefined;
            }
            set
            {
                if (key == null) throw new ArgumentNullException(nameof(key));
                if (value.Is(null))
                    _values.Remove(key);
                else
                    _values[key] = value;
            }
        }

        #region Implementation of IEnumerable

        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        ///     A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<KeyValuePair<string, EarleValue>> GetEnumerator()
        {
            return _values.GetEnumerator();
        }

        /// <summary>
        ///     Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        ///     An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}