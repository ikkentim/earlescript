// Archer
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
using EarleCode.Runtime.Attributes;
using EarleCode.Runtime.Instructions;
using EarleCode.Runtime.Operators;
using EarleCode.Runtime.Values;

namespace EarleCode.Runtime
{
    public partial class EarleRuntime
    {
        private void RegisterDefaultOperators()
        {
            // @
            Operators.AddUnaryOperator(OpCode.Convert, typeof(string),
                v => (EarleValue) (Localizer.Localize((string) v)));
        }

        private static int CompareFloatInt(object val1, object val2)
        {
            if (!(val1 is float || val1 is int))
                throw new ArgumentException("value must be int or float", nameof(val1));
            if (!(val2 is float || val2 is int))
                throw new ArgumentException("value must be int or float", nameof(val2));

            if (val1 is int && val2 is int)
                return ((int) val1).CompareTo(val2);

            if (val1 is float)
                return ((float) val1).CompareTo((int) val2);

            return ((float) (int) val1).CompareTo(val2);
        }

        #region Binary Operators

        [EarleBinaryOperator(OpCode.Add, null, typeof(string), EarleOperatorParamOrder.Any)]
        private static EarleValue AddOs(EarleValue l, EarleValue r) => (EarleValue) ((string) l + (string) r);

        [EarleBinaryOperator(OpCode.Add, typeof(int), typeof(float), EarleOperatorParamOrder.Specified)]
        private static EarleValue AddIf(EarleValue l, EarleValue r) => (EarleValue) ((int) l + (float) r);

        [EarleBinaryOperator(OpCode.Add, typeof(int))]
        private static EarleValue AddIi(EarleValue l, EarleValue r) => (EarleValue) ((int) l + (int) r);

        [EarleBinaryOperator(OpCode.Add, typeof(float))]
        private static EarleValue AddFf(EarleValue l, EarleValue r) => (EarleValue) ((float) l + (float) r);

        [EarleBinaryOperator(OpCode.Add, typeof(EarleVector3))]
        private static EarleValue AddV3V3(EarleValue l, EarleValue r)
            => (l.As<EarleVector3>() + r.As<EarleVector3>()).ToEarleValue();

        // -
        [EarleBinaryOperator(OpCode.Subtract, typeof(int), typeof(float))]
        private static EarleValue SubtractIf(EarleValue l, EarleValue r) => (EarleValue) ((int) l - (float) r);

        [EarleBinaryOperator(OpCode.Subtract, typeof(float), typeof(int))]
        private static EarleValue SubtractFi(EarleValue l, EarleValue r) => (EarleValue) ((float) l - (int) r);

        [EarleBinaryOperator(OpCode.Subtract, typeof(float), typeof(float))]
        private static EarleValue SubtractFf(EarleValue l, EarleValue r) => (EarleValue) ((float) l - (float) r);

        [EarleBinaryOperator(OpCode.Subtract, typeof(int), typeof(int))]
        private static EarleValue SubtractIi(EarleValue l, EarleValue r) => (EarleValue) ((int) l - (int) r);

        // *
        [EarleBinaryOperator(OpCode.Multiply, typeof(int), typeof(float), EarleOperatorParamOrder.Specified)]
        private static EarleValue MultiplyIf(EarleValue l, EarleValue r) => (EarleValue) ((int) l*(float) r);


        [EarleBinaryOperator(OpCode.Multiply, typeof(int), typeof(int))]
        private static EarleValue MultiplyIi(EarleValue l, EarleValue r) => (EarleValue) ((int) l*(int) r);

        [EarleBinaryOperator(OpCode.Multiply, typeof(float), typeof(float))]
        private static EarleValue MultiplyFf(EarleValue l, EarleValue r) => (EarleValue) ((float) l*(float) r);

        // /
        [EarleBinaryOperator(OpCode.Divide, typeof(int), typeof(float))]
        private static EarleValue DivideIf(EarleValue l, EarleValue r) => (EarleValue) ((int) l/(float) r);

        [EarleBinaryOperator(OpCode.Divide, typeof(float), typeof(int))]
        private static EarleValue DivideFi(EarleValue l, EarleValue r) => (EarleValue) ((float) l/(int) r);

        [EarleBinaryOperator(OpCode.Divide, typeof(float), typeof(float))]
        private static EarleValue DivideFf(EarleValue l, EarleValue r) => (EarleValue) ((float) l/(float) r);

        [EarleBinaryOperator(OpCode.Divide, typeof(int), typeof(int))]
        private static EarleValue DivideIi(EarleValue l, EarleValue r) => (EarleValue) ((int) l/(int) r);

        // %
        [EarleBinaryOperator(OpCode.Modulo, typeof(int), typeof(int))]
        private static EarleValue Modulo(EarleValue l, EarleValue r) => (EarleValue) ((int) l%(int) r);

        // ^
        [EarleBinaryOperator(OpCode.BitwiseXor, typeof(int), typeof(int))]
        private static EarleValue Xor(EarleValue l, EarleValue r) => (EarleValue) ((int) l ^ (int) r);

        // |
        [EarleBinaryOperator(OpCode.BitwiseOr, typeof(int), typeof(int))]
        private static EarleValue BitwiseOr(EarleValue l, EarleValue r) => (EarleValue) ((int) l | (int) r);

        // &
        [EarleBinaryOperator(OpCode.BitwiseAnd, typeof(int), typeof(int))]
        private static EarleValue BitwiseAnd(EarleValue l, EarleValue r) => (EarleValue) ((int) l & (int) r);

        // <<
        [EarleBinaryOperator(OpCode.ShiftLeft, typeof(int), typeof(int))]
        private static EarleValue ShiftLeft(EarleValue l, EarleValue r) => (EarleValue) ((int) l << (int) r);

        // >>
        [EarleBinaryOperator(OpCode.ShiftRight, typeof(int), typeof(int))]
        private static EarleValue ShiftRight(EarleValue l, EarleValue r) => (EarleValue) ((int) l >> (int) r);

        // <
        [EarleBinaryOperator(OpCode.CheckLessThan, new[] {typeof(float), typeof(int)})]
        private static EarleValue CheckLessThan(EarleValue l, EarleValue r)
            => (EarleValue) (CompareFloatInt(l.Value, r.Value) < 0);

        // >
        [EarleBinaryOperator(OpCode.CheckGreaterThan, new[] {typeof(float), typeof(int)})]
        private static EarleValue CheckGreaterThan(EarleValue l, EarleValue r)
            => (EarleValue) (CompareFloatInt(l.Value, r.Value) > 0);

        // <=
        [EarleBinaryOperator(OpCode.CheckLessOrEqual, new[] {typeof(float), typeof(int)})]
        private static EarleValue CheckLessOrEqual(EarleValue l, EarleValue r)
            => (EarleValue) (CompareFloatInt(l.Value, r.Value) <= 0);

        // >=
        [EarleBinaryOperator(OpCode.CheckGreaterOrEqual, new[] {typeof(float), typeof(int)})]
        private static EarleValue CheckGreaterOrEqual(EarleValue l, EarleValue r)
            => (EarleValue) (CompareFloatInt(l.Value, r.Value) >= 0);

        // ==
        [EarleBinaryOperator(OpCode.CheckEqual, new[] {typeof(float), typeof(int)})]
        private static EarleValue CheckEqualFi(EarleValue l, EarleValue r)
            => (EarleValue) (CompareFloatInt(l.Value, r.Value) == 0);

        [EarleBinaryOperator(OpCode.CheckEqual, (Type) null, null)]
        private static EarleValue CheckEqualOo(EarleValue l, EarleValue r)
        {
            return (EarleValue) (l.Value?.Equals(r.Value) ?? r.Value == null);
        }

        // !=
        [EarleBinaryOperator(OpCode.CheckNotEqual, new[] {typeof(float), typeof(int)})]
        private static EarleValue CheckNotEqualFi(EarleValue l, EarleValue r)
            => (EarleValue) (CompareFloatInt(l.Value, r.Value) == 0);

        [EarleBinaryOperator(OpCode.CheckNotEqual, (Type) null, null)]
        private static EarleValue CheckNotEqualOo(EarleValue l, EarleValue r)
        {
            return (EarleValue) (!l.Value?.Equals(r.Value) ?? r.Value != null);
        }

        #endregion

        #region Unary Operators

        // -
        [EarleUnaryOperator(OpCode.Negate, typeof(int))]
        private static EarleValue NegateI(EarleValue v) => (EarleValue) (-(int) v);

        [EarleUnaryOperator(OpCode.Negate, typeof(float))]
        private static EarleValue NegateF(EarleValue v) => (EarleValue) (-(float) v);

        // !
        [EarleUnaryOperator(OpCode.LogicalNot, typeof(int))]
        private static EarleValue LogicalNot(EarleValue v) => (EarleValue) ((int) v != 0);

        // ~
        [EarleUnaryOperator(OpCode.BitwiseNot, typeof(int))]
        private static EarleValue BitwiseNot(EarleValue v) => (EarleValue) (~(int) v);

        #endregion
    }
}