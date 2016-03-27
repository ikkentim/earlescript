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
using System.Globalization;

namespace EarleCode.Retry
{
    public struct EarleValue
    {
        public EarleValue(int value) : this()
        {
            Value = value;
            Type = EarleValueType.Integer;
        }

        public EarleValue(float value) : this()
        {
            Value = value;
            Type = EarleValueType.Float;
        }

        public EarleValue(string value) : this()
        {
            Value = value;
            Type = EarleValueType.String;
        }

        public EarleValue(EarleFunction function) : this()
        {
            Value = function;
            Type = EarleValueType.Function;
        }

        public EarleValue(EarleVariableReference value) : this()
        {
            Value = value;
            Type = EarleValueType.Reference;
        }

        public EarleValue(object value, EarleValueType type) : this()
        {
            Value = value;
            Type = type;
        }

        public object Value { get; }

        public EarleValueType Type { get; }

        [DebuggerHidden]
        public void AssertOfType(EarleValueType type)
        {
            if (Type != type)
                throw new Exception("Invalid value type");
        }

        [DebuggerHidden]
        private static EarleValueType GetValueTypeForType(Type type)
        {
            if (type == typeof (int))
                return EarleValueType.Integer;
            if (type == typeof (float))
                return EarleValueType.Float;
            if (type == typeof (string))
                return EarleValueType.String;
            if (type == typeof (EarleVariableReference))
                return EarleValueType.Reference;
            if (type == typeof (EarleFunction))
                return EarleValueType.Function;

            throw new Exception("Unsupported value type");
        }

        #region To

        public T To<T>()
        {
            return (T) To(typeof (T));
        }

        public object To(Type type)
        {
            return To(GetValueTypeForType(type));
        }

        public object To(EarleValueType type)
        {
            if (Is(type))
                return Value;

            switch (type)
            {
                case EarleValueType.Float:
                    if (Is<int>())
                        return (float) As<int>();
                    break;
                case EarleValueType.Integer:
                    if (Is<float>())
                        return (int) As<float>();
                    break;
                case EarleValueType.String:
                    if (Is<int>()) return As<int>().ToString();
                    if (Is<float>()) return As<float>().ToString(CultureInfo.InvariantCulture);
                    break;
            }

            throw new Exception("Invalid cast");
        }

        #endregion

        #region As

        public T As<T>()
        {
            return (T) As(typeof (T));
        }

        public object As(Type type)
        {
            return As(GetValueTypeForType(type));
        }

        public object As(EarleValueType type)
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
            return Is(GetValueTypeForType(type));
        }

        public bool Is(EarleValueType type)
        {
            return Type == type;
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
            return "EarleValue container filled with " + (Value?.ToString() ?? Type.ToString());
        }

        #endregion
    }
}