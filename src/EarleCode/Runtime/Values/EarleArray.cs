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

using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace EarleCode.Runtime.Values
{
    /// <summary>
    /// Represents an array of Earle values.
    /// </summary>
    /// <seealso cref="EarleCode.Runtime.Values.IEarleStructure" />
    public class EarleArray : IEarleStructure, IEnumerable<KeyValuePair<EarleValue, EarleValue>>
    {
        private readonly IDictionary<object, EarleValue> _values = new Dictionary<object, EarleValue>();

        /// <summary>
        /// Gets the length of this array.
        /// </summary>
        public virtual int Length => _values.Count;

        /// <summary>
        /// Gets the value at the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>The value at the specified index. If the index is out of range, <see cref="EarleValue.Undefined"/> is returned.</returns>
        public virtual EarleValue GetValue(EarleValue index)
        {
            if (!index.HasValue)
                return EarleValue.Undefined;

            EarleValue value;
            return _values.TryGetValue(index.Value, out value)
                ? value
                : EarleValue.Undefined;
        }

        /// <summary>
        /// Sets the value at the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="value">The value.</param>
        public virtual void SetValue(EarleValue index, EarleValue value)
        {
            if (!index.HasValue)
                return;

            if (value.HasValue)
                _values[index.Value] = value;
            else
                _values.Remove(index.Value);
        }

        #region Implementation of IEarleStructure

        public EarleValue GetField(string name)
        {
            return name == "length" ? (EarleValue) Length : EarleValue.Undefined;
        }

        public void SetField(string name, EarleValue value)
        {
            // Fields of arrays can't be set.
        }

        #endregion

        #region Implementation of IEnumerable

        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        ///     A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<KeyValuePair<EarleValue, EarleValue>> GetEnumerator()
        {
            return
                _values.Select(kv => new KeyValuePair<EarleValue, EarleValue>(kv.Key.ToEarleValue(), kv.Value))
                    .GetEnumerator();
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