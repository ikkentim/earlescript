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

namespace EarleCode.Runtime.Values.ValueTypes
{
    public abstract class EarleValueType<T> : IEarleValueType
    {
        protected abstract object CastToOtherType(Type toType, T value);

        #region Implementation of IEarleValueType

        public virtual Type Type { get; } = typeof (T);

        public virtual object CastTo<TTo>(EarleValue value)
        {
            return CastTo(typeof(TTo), value);
        }

        public virtual object CastTo(Type toType, EarleValue value)
        {
            if(value.Value?.GetType() != Type)
            {
                throw new ArgumentException("Value must be of the type specified by the Type property in order for it to be castable by this instance.\t", nameof(value));
            }
            if(Type == toType)
                return value.Value;

            return CastToOtherType(toType, (T)value.Value);
        }
        #endregion
    }
}