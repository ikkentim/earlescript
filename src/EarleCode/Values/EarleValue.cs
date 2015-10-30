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

using System;
using EarleCode.Functions;

namespace EarleCode.Values
{
    public struct EarleValue
    {
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
                 if(Value is EarleArray) return EarleValueType.Array;
                if (Value is EarleVector) return EarleValueType.Vector;
                if (Value is EarleFunction) return EarleValueType.Function;
                if (Value is IEarleContext) return EarleValueType.Context;
                if (Value is IVariable) return EarleValueType.Variable;
                return EarleValueType.Void;
            }
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
                    return -(int) value.Value;
                case EarleValueType.Float:
                    return -(float) value.Value;
                default:
                    return Null;
            }
        }

        public static EarleValue Subtract(EarleValue leftValue, EarleValue rightValue)
        {
            var leftType = leftValue.Type;
            var rightType = rightValue.Type;

            if (leftType == EarleValueType.Float || rightType == EarleValueType.Float)
                return (float) leftValue.CastTo(EarleValueType.Float).Value -
                       (float) rightValue.CastTo(EarleValueType.Float).Value;

            return (int) leftValue.CastTo(EarleValueType.Integer).Value -
                   (int) rightValue.CastTo(EarleValueType.Integer).Value;
        }

        public static EarleValue Add(EarleValue leftValue, EarleValue rightValue)
        {
            var leftType = leftValue.Type;
            var rightType = rightValue.Type;

            if (leftType == EarleValueType.String || rightType == EarleValueType.String)
                return (string) leftValue.CastTo(EarleValueType.String).Value +
                       (string) rightValue.CastTo(EarleValueType.String).Value;

            if (leftType == EarleValueType.Float || rightType == EarleValueType.Float)
                return (float) leftValue.CastTo(EarleValueType.Float).Value +
                       (float) rightValue.CastTo(EarleValueType.Float).Value;

            return (int) leftValue.CastTo(EarleValueType.Integer).Value +
                   (int) rightValue.CastTo(EarleValueType.Integer).Value;
        }

        public static EarleValueType GetType(Type type)
        {
            if (type == typeof (IEarleContext)) return EarleValueType.Context;
            if (type == typeof (EarleArray)) return EarleValueType.Array;
            if (type == typeof (float)) return EarleValueType.Float;
            if (type == typeof (EarleFunctionSignature)) return EarleValueType.Function;
            if (type == typeof (int)) return EarleValueType.Integer;
            if (type == typeof (string)) return EarleValueType.String;
            if (type == typeof (IVariable)) return EarleValueType.Variable;
            if (type == typeof (EarleVector)) return EarleValueType.Vector;

            return EarleValueType.Void;
        }

        public T Cast<T>()
        {
            var value = CastTo(GetType(typeof (T))).Value;
            return value is T ? (T) value : default(T);
        }

        public EarleValue CastTo(EarleValueType targetType)
        {
            if (Type == targetType)
                return this;

            switch (targetType)
            {
                case EarleValueType.Integer:
                    switch (Type)
                    {
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
                            return (float) (int) Value;
                        case EarleValueType.String:
                            float result;
                            return float.TryParse((string) Value, out result) ? result : Null;
                        default:
                            return 0f;
                    }
                case EarleValueType.String:
                    return Type == EarleValueType.Void ? string.Empty : Value.ToString();
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
                case EarleValueType.Vector:
                    var vector = (EarleVector) Value;
                    return vector.X != 0 || vector.Y != 0 || vector.Z != 0;
                case EarleValueType.Function:
                case EarleValueType.Array:
                case EarleValueType.Context:
                    return true;
                default:
                    return false;
            }
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
        public static implicit operator EarleValue(EarleVector value) => new EarleValue(value);
        public static explicit operator int (EarleValue value) => (int)value.CastTo(EarleValueType.Integer).Value;
        public static explicit operator float (EarleValue value) => (float)value.CastTo(EarleValueType.Float).Value;
        public static explicit operator string (EarleValue value) => (string)value.CastTo(EarleValueType.String).Value;
        public static explicit operator EarleVector (EarleValue value) => (EarleVector)value.CastTo(EarleValueType.Vector).Value;
        public static explicit operator EarleArray(EarleValue value) => (EarleArray)value.CastTo(EarleValueType.Array).Value;
        public static explicit operator EarleFunction (EarleValue value) => (EarleFunction)value.CastTo(EarleValueType.Function).Value;
    }
}