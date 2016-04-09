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

namespace EarleCode.Values.ValueTypes
{
    public abstract class EarleValueType<T> : IEarleValueType
    {
        protected abstract T ParseOtherValueToType(EarleValue value);

        #region Implementation of IEarleValueType

        public virtual Type Type { get; } = typeof (T);

        public virtual object ParseValueToType(EarleValue value)
        {
            if (value.Is(Type)) return value.Value;

            var parsedValue = ParseOtherValueToType(value);

            return parsedValue != null ? (object) parsedValue : null;
        }

        #endregion
    }
}