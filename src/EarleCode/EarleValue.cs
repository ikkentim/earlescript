// EarleCode
// Copyright 2015 Tim Potze
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

using System.Diagnostics;
using System.Runtime.CompilerServices;
using EarleCode.Functions;

namespace EarleCode
{
    public struct EarleValue
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="T:System.Object" /> class.
        /// </summary>
        public EarleValue(object value)
        {
            Value = value;
            // TODO Type check
        }

        public static EarleValue Null { get; } = new EarleValue();
        public object Value { get; set; }

        public EarleValueType Type
        {
            get
            {
                if (Value is int) return EarleValueType.Integer;
                if (Value is float) return EarleValueType.Float;
                if (Value is string) return EarleValueType.String;
//              if(Value is array) return EarleValueType.Array;
                if (Value is EarleFunction) return EarleValueType.Function;
                return EarleValueType.Void;
            }
        }

        public static EarleValue Subtract(EarleValue leftValue, EarleValue rightValue)
        {
            var leftType = leftValue.Type;
            var rightType = rightValue.Type;

            if (leftType == EarleValueType.Float || rightType == EarleValueType.Float)
                return (float)leftValue.CastTo(EarleValueType.Float).Value -
                       (float)rightValue.CastTo(EarleValueType.Float).Value;

            return (int)leftValue.CastTo(EarleValueType.Integer).Value -
                   (int)rightValue.CastTo(EarleValueType.Integer).Value;
        }

        public static EarleValue Positive(EarleValue value)
        {
            switch (value.Type)
            {
                case EarleValueType.Integer:
                case EarleValueType.Float:
                    return value;
                default:
                    return Null;
            }

        }

        public static EarleValue Negative(EarleValue value)
        {
            switch (value.Type)
            {
                case EarleValueType.Integer:
                    return -(int)value.Value;
                case EarleValueType.Float:
                    return -(float)value.Value;
                default:
                    return Null;
            }

        }

        public static EarleValue Add(EarleValue leftValue, EarleValue rightValue)
        {
            var leftType = leftValue.Type;
            var rightType = rightValue.Type;

            if (leftType == EarleValueType.String || rightType == EarleValueType.String)
                return (string)leftValue.CastTo(EarleValueType.String).Value +
                       (string)rightValue.CastTo(EarleValueType.String).Value;

            if (leftType == EarleValueType.Float || rightType == EarleValueType.Float)
                return (float)leftValue.CastTo(EarleValueType.Float).Value +
                       (float)rightValue.CastTo(EarleValueType.Float).Value;

            return (int)leftValue.CastTo(EarleValueType.Integer).Value +
                   (int)rightValue.CastTo(EarleValueType.Integer).Value;
        }

        public EarleValue CastTo(EarleValueType targetType)
        {
            switch (targetType)
            {
                case EarleValueType.Integer:
                    switch (Type)
                    {
                        case EarleValueType.Integer:
                            return this;
                        case EarleValueType.Float:
                            return (int) (float) Value;
                        case EarleValueType.String:
                            int result;
                            return int.TryParse((string) Value, out result) ? result : Null;
                        default:
                            return 0;
                    }
                case EarleValueType.Float:
                    switch (Type)
                    {
                        case EarleValueType.Integer:
                            return (float)(int)Value;
                        case EarleValueType.Float:
                            return this;
                        case EarleValueType.String:
                            float result;
                            return float.TryParse((string)Value, out result) ? result : Null;
                        default:
                            return 0f;
                    }
                case EarleValueType.String:
                    return Type == EarleValueType.String ? this : (Type == EarleValueType.Void ? string.Empty : Value.ToString());
                case EarleValueType.Array:
                    return Type == EarleValueType.Array ? this : Null;
                case EarleValueType.Function:
                    return Type == EarleValueType.Function ? this : Null;
                case EarleValueType.Void:
                default:
                    return Null;
            }
        }

        public bool ToBoolean()
        {
            switch (Type)
            {
                case EarleValueType.Integer:
                    return (int) Value != 0;
                case EarleValueType.Float:
                    return (float) Value != 0f;
                case EarleValueType.String:
                    return !string.IsNullOrEmpty((string) Value);
                case EarleValueType.Function:
                    return true;
                default:
                    return false;
            }
        }

        #region Overrides of ValueType

        /// <summary>
        /// Returns the fully qualified type name of this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> containing a fully qualified type name.
        /// </returns>
        public override string ToString()
        {
            switch (Type)
            {
                case EarleValueType.String:
                    return $"\"{Value}\"";
                case EarleValueType.Void:
                    return "NULL";
                default:
                    return Value.ToString();
            }
        }

        #endregion

        public static implicit operator EarleValue(int value) => new EarleValue(value);
        public static implicit operator EarleValue(float value) => new EarleValue(value);
        public static implicit operator EarleValue(string value) => new EarleValue(value);
        public static implicit operator EarleValue(EarleFunction value) => new EarleValue(value);
    }
}