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
using System.Diagnostics;
using System.Linq;

namespace EarleCode.Runtime.Values
{
    public struct EarleValue
    {
        public EarleValue(object value) : this()
        {
            if (value is bool)
                value = (bool) value ? 1 : 0;

            Value = value;
        }

        public static readonly EarleValue Undefined = new EarleValue();

        public static EarleValue True { get; } = new EarleValue(1);

        public static EarleValue False { get; } = new EarleValue(0);

        public object Value { get; }

        public bool HasValue => Value != null;

        #region To

        public T CastTo<T>()
        {
            if (Is<T>())
                return As<T>();

            var result = CastTo(typeof (T));
            return (T) (result ?? default(T));
        }

        public object CastTo(Type type)
        {
            var caster = EarleValueTypeStore.GetCaster(Value?.GetType(), type);

            if (caster == null)
                return null;

            return caster(Value);
        }

        #endregion

        #region As

        [DebuggerHidden]
        public void AssertOfType(params Type[] types)
        {
            if (types == null) throw new ArgumentNullException(nameof(types));

            if (!IsAny(types))
                throw new Exception("Unexpected earle value type");
        }

        public T As<T>()
        {
            return Value is T ? (T) Value : default(T);
        }

        public object As(Type type)
        {
            return Is(type) ? Value : null;
        }

        #endregion

        #region Is

        public bool Is<T>()
        {
            return Value is T;
        }

        public bool Is(Type type)
        {
            return type?.IsInstanceOfType(Value) ?? Value == null;
        }

        public bool IsAny(params Type[] types)
        {
            return types != null && types.Length != 0 && types.Any(Is);
        }

        #endregion

        public static explicit operator int(EarleValue value)
        {
            return value.CastTo<int>();
        }

        public static explicit operator float(EarleValue value)
        {
            return value.CastTo<float>();
        }

        public static explicit operator string(EarleValue value)
        {
            return value.CastTo<string>();
        }

        public static explicit operator bool(EarleValue value)
        {
            return value.CastTo<bool>();
        }

        public static explicit operator EarleValue(bool value)
        {
            return value ? True : False;
        }

        public static explicit operator EarleValue(int value)
        {
            return new EarleValue(value);
        }

        public static explicit operator EarleValue(float value)
        {
            return new EarleValue(value);
        }

        public static explicit operator EarleValue(string value)
        {
            return new EarleValue(value);
        }

        #region Overrides of ValueType

        /// <summary>
        ///     Returns the fully qualified type name of this instance.
        /// </summary>
        /// <returns>
        ///     A <see cref="T:System.String" /> containing a fully qualified type name.
        /// </returns>
        public override string ToString()
        {
            return $@"EarleValue (Value = ""{Value?.ToString() ?? "undefined"}"")";
        }

        #endregion
    }
}