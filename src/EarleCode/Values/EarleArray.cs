﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace EarleCode.Values
{
    public class EarleArray : IEarleStructure, IEnumerable<KeyValuePair<EarleValue, EarleValue>>
    {
        private readonly IDictionary<object,EarleValue> _values = new Dictionary<object, EarleValue>();
        
        public virtual EarleValue GetValue(EarleValue index)
        {
            if(!index.HasValue)
                return EarleValue.Undefined;

            EarleValue value;
            return _values.TryGetValue(index.Value, out value)
                ? value
                : EarleValue.Undefined;
        }
        
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
            return name == "length" ? _values.Count.ToEarleValue() : EarleValue.Undefined;
        }

        public void SetField(string name, EarleValue value)
        {

        }

        #endregion

        #region Implementation of IEnumerable

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<KeyValuePair<EarleValue, EarleValue>> GetEnumerator()
        {
            return
                _values.Select(kv => new KeyValuePair<EarleValue, EarleValue>(kv.Key.ToEarleValue(), kv.Value))
                    .GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}