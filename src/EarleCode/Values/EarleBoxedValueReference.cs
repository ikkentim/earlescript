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

namespace EarleCode.Values
{
    public struct EarleBoxedValueReference
    {
        private readonly EarleArray _array;
        private readonly EarleValue _index;
        private readonly bool _isArray;
        private readonly IEarleStructure _target;
        private readonly string _fieldName;

        public EarleBoxedValueReference(IEarleStructure target, string fieldName) : this()
        {
            if (target == null) throw new ArgumentNullException(nameof(target));
            if (fieldName == null) throw new ArgumentNullException(nameof(fieldName));
            _isArray = false;
            _target = target;
            _fieldName = fieldName;
        }

        public EarleBoxedValueReference(EarleArray array, EarleValue index) : this()
        {
            if (array == null) throw new ArgumentNullException(nameof(array));
            _isArray = true;
            _array = array;
            _index = index;
        }

        public EarleValue GetField()
        {
            return _isArray
                ? _array.GetValue(_index)
                : _target.GetField(_fieldName);
        }

        public void SetField(EarleValue value)
        {
            if (_isArray)
                _array.SetValue(_index, value);
            else
                _target.SetField(_fieldName, value);
        }
    }
    
}