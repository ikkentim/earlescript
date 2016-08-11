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

namespace EarleCode.Runtime.Values
{
	public class EarleStructure : IEarleStructure, IEnumerable<KeyValuePair<string, EarleValue>>
	{
		private readonly EarleDictionary _values = new EarleDictionary();

		public EarleValue this[string key]
		{
			get { return GetField(key); }
			set { SetField(key, value); }
		}

		public int Count => _values.Count;

		public bool IsReadOnly => false;

		public ICollection<string> Keys => _values.Keys;

		public ICollection<EarleValue> Values => _values.Values;

		public void Clear() => _values.Clear();

		public bool ContainsKey(string key) => _values.ContainsKey(key);

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

		#region Implementation of IEnumerable<KeyValuePair<string,EarleValue>>

		public IEnumerator<KeyValuePair<string, EarleValue>> GetEnumerator() => _values.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		#endregion
	}
}