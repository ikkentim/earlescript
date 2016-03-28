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

namespace EarleCode.Values
{
    public struct EarleValue
    {
        public EarleValue(object value) : this()
        {
            Value = value;
        }

        public static EarleValue Null { get; } = new EarleValue();

        public static EarleValue True { get; } = new EarleValue(1);

        public static EarleValue False { get; } = new EarleValue(0);

        public object Value { get; }

        #region To

        public T To<T>(Runtime runtime)
        {
            if (runtime == null) throw new ArgumentNullException(nameof(runtime));
            return (T) To(typeof (T), runtime);
        }

        public object To(Type type, Runtime runtime)
        {
            if (runtime == null) throw new ArgumentNullException(nameof(runtime));
            var valueType = runtime.GetValueTypeForType(type);

            return valueType?.ParseValueToType(this).Value;
        }

        #endregion

        #region As

        [DebuggerHidden]
        public void AssertOfType<T>()
        {
            AssertOfType(typeof (T));
        }

        [DebuggerHidden]
        public void AssertOfType(Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            if (!Is(type))
                throw new Exception("Unexpected earle value type");
        }

        [DebuggerHidden]
        public void AssertOfType(params Type[] types)
        {
            if (types == null) throw new ArgumentNullException(nameof(types));

            if (!IsAny(types))
                throw new Exception("Unexpected earle value type");
        }

        public T As<T>()
        {
            return (T) As(typeof (T));
        }

        public object As(Type type)
        {
            AssertOfType(type);
            return Value;
        }

        #endregion

        #region Is

        public bool Is<T>()
        {
            return Is(typeof (T));
        }

        public bool Is(Type type)
        {
            if (type == null)
                return Value == null;

            return type.IsInstanceOfType(Value);
        }

        public bool IsAny(params Type[] types)
        {
            return types != null && types.Length != 0 && types.Any(Is);
        }

        #endregion

        #region Overrides of ValueType

        /// <summary>
        ///     Returns the fully qualified type name of this instance.
        /// </summary>
        /// <returns>
        ///     A <see cref="T:System.String" /> containing a fully qualified type name.
        /// </returns>
        public override string ToString()
        {
            return $@"EarleValue (Value = ""{Value?.ToString() ?? "NULL"}"")";
        }

        #endregion
    }
}