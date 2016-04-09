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
    public interface IEarleStructure : IEnumerable<KeyValuePair<String, EarleValue>>
    {
        EarleValue GetField(string name);
        void SetField(string name, EarleValue value);
    }

    public class EarleBasicStructure : IEarleStructure
    {
        private readonly EarleDictionary _values = new EarleDictionary();

        #region Implementation of IEarleStructure

        public virtual EarleValue GetField(string name)
        {
            return _values[name];
        }

        public virtual void SetField(string name, EarleValue value)
        {
            _values[name] = value;
        }

        #endregion

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

    public struct EarleBoxedField
    {
        public EarleBoxedField(IEarleStructure target, string fieldName)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));
            if (fieldName == null) throw new ArgumentNullException(nameof(fieldName));
            Target = target;
            FieldName = fieldName;
        }

        public IEarleStructure Target { get; }

        public string FieldName { get; }

        public EarleValue GetField()
        {
            return Target.GetField(FieldName);
        }

        public void SetField(EarleValue value)
        {
            Target.SetField(FieldName, value);
        }
    }
}