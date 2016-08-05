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
using EarleCode.Runtime.Instructions;
using EarleCode.Runtime.Operators;
using EarleCode.Runtime.Values;

namespace EarleCode.Runtime
{
    public partial class EarleRuntime
    {
        private void RegisterDefaultOperators()
        {
            // BINARY OPERATORS
       
            // +
            Operators.AddBinaryOperator(OpCode.Add, null, typeof(string), (l, r) => {
                return (EarleValue)((string)l + (string)r);
            }, EarleOperatorTypeOrder.Any);

            Operators.AddBinaryOperator(OpCode.Add, typeof(int), typeof(float), (l, r) => {
                return (EarleValue)((int)l + (float)r);
            }, EarleOperatorTypeOrder.Specified);

            Operators.AddBinaryOperator(OpCode.Add, typeof(int), typeof(int), (l, r) => {
                return (EarleValue)((int)l + (int)r);
            });

            Operators.AddBinaryOperator(OpCode.Add, typeof(float), typeof(float), (l, r) => {
                return (EarleValue)((float)l + (float)r);
            });

            // -
            Operators.AddBinaryOperator(OpCode.Subtract, typeof(int), typeof(float), (l, r) => {
                return (EarleValue)((int)l - (float)r);
            });

            Operators.AddBinaryOperator(OpCode.Subtract, typeof(float), typeof(int), (l, r) => {
                return (EarleValue)((float)l - (int)r);
            });

            Operators.AddBinaryOperator(OpCode.Subtract, typeof(float), typeof(float), (l, r) => {
                return (EarleValue)((float)l - (float)r);
            });

            Operators.AddBinaryOperator(OpCode.Subtract, typeof(int), typeof(int), (l, r) => {
                return (EarleValue)((int)l - (int)r);
            });

            // *
            Operators.AddBinaryOperator(OpCode.Multiply, typeof(int), typeof(float), (l, r) => {
                return (EarleValue)((int)l * (float)r);
            }, EarleOperatorTypeOrder.Specified);


            Operators.AddBinaryOperator(OpCode.Multiply, typeof(int), typeof(int), (l, r) => {
                return (EarleValue)((int)l * (int)r);
            });

            Operators.AddBinaryOperator(OpCode.Multiply, typeof(float), typeof(float), (l, r) => {
                return (EarleValue)((float)l * (float)r);
            });

            // /
            Operators.AddBinaryOperator(OpCode.Divide, typeof(int), typeof(float), (l, r) => {
                return (EarleValue)((int)l / (float)r);
            });

            Operators.AddBinaryOperator(OpCode.Divide, typeof(float), typeof(int), (l, r) => {
                return (EarleValue)((float)l / (int)r);
            });

            Operators.AddBinaryOperator(OpCode.Divide, typeof(float), typeof(float), (l, r) => {
                return (EarleValue)((float)l / (float)r);
            });

            Operators.AddBinaryOperator(OpCode.Divide, typeof(int), typeof(int), (l, r) => {
                return (EarleValue)((int)l / (int)r);
            });

            // %
            Operators.AddBinaryOperator(OpCode.Modulo, typeof(int), typeof(int), (l, r) => {
                return (EarleValue)((int)l % (int)r);
            });

            // ^
            Operators.AddBinaryOperator(OpCode.BitwiseXor, typeof(int), typeof(int), (l, r) => {
                return (EarleValue)((int)l ^ (int)r);
            });

            // |
            Operators.AddBinaryOperator(OpCode.BitwiseOr, typeof(int), typeof(int), (l, r) => {
                return (EarleValue)((int)l | (int)r);
            });

            // &
            Operators.AddBinaryOperator(OpCode.BitwiseAnd, typeof(int), typeof(int), (l, r) => {
                return (EarleValue)((int)l & (int)r);
            });

            // <<
            Operators.AddBinaryOperator(OpCode.ShiftLeft, typeof(int), typeof(int), (l, r) => {
                return (EarleValue)((int)l << (int)r);
            });

            // >>
            Operators.AddBinaryOperator(OpCode.ShiftRight, typeof(int), typeof(int), (l, r) => {
                return (EarleValue)((int)l >> (int)r);
            });

            // <
            Operators.AddBinaryOperator(OpCode.CheckLessThan, new[] { typeof(float), typeof(int) }, (l, r) => {
                return (EarleValue)(CompareFloatInt(l.Value, r.Value) < 0);
            });

            // >
            Operators.AddBinaryOperator(OpCode.CheckGreaterThan, new[] { typeof(float), typeof(int) }, (l, r) => {
                return (EarleValue)(CompareFloatInt(l.Value, r.Value) > 0);
            });

            // <=
            Operators.AddBinaryOperator(OpCode.CheckLessOrEqual, new[] { typeof(float), typeof(int) }, (l, r) => {
                return (EarleValue)(CompareFloatInt(l.Value, r.Value) <= 0);
            });

            // >=
            Operators.AddBinaryOperator(OpCode.CheckGreaterOrEqual, new[] { typeof(float), typeof(int) }, (l, r) => {
                return (EarleValue)(CompareFloatInt(l.Value, r.Value) >= 0);
            });

            // ==
            Operators.AddBinaryOperator(OpCode.CheckEqual, new[] { typeof(float), typeof(int) }, (l, r) => {
                return (EarleValue)(CompareFloatInt(l.Value, r.Value) == 0);
            });

            Operators.AddBinaryOperator(OpCode.CheckEqual, null as Type, null, (l, r) => {
                return (EarleValue)(l.Value == null
                    ? r.Value == null
                    : l.Value.Equals(r.Value));
            });

            // !=
            Operators.AddBinaryOperator(OpCode.CheckNotEqual, new[] { typeof(float), typeof(int) }, (l, r) => {
                return (EarleValue)(CompareFloatInt(l.Value, r.Value) != 0);
            });

            Operators.AddBinaryOperator(OpCode.CheckNotEqual, null as Type, null, (l, r) => {
                return (EarleValue)(l.Value == null
                    ? r.Value != null
                    : !l.Value.Equals(r.Value));
            });

            // UNARY OPERATORS

            // -
            Operators.AddUnaryOperator(OpCode.Negate, typeof(int), v => (EarleValue)(-(int)v));
            Operators.AddUnaryOperator(OpCode.Negate, typeof(float), v => (EarleValue)(-(float)v));

            // !
            Operators.AddUnaryOperator(OpCode.LogicalNot, typeof(int), v => (EarleValue)((int)v != 0));

            // ~
            Operators.AddUnaryOperator(OpCode.BitwiseNot, typeof(int), v => (EarleValue)(~(int)v));

            // @
            Operators.AddUnaryOperator(OpCode.Convert, typeof(string), v => (EarleValue)(Localizer.Localize((string)v)));
        }

        private static int CompareFloatInt(object val1, object val2)
        {
            if(!(val1 is float || val1 is int))
                throw new ArgumentException("value must be int or float", nameof(val1));
            if(!(val2 is float || val2 is int))
                throw new ArgumentException("value must be int or float", nameof(val2));
            
            if(val1 is int && val2 is int)
                return ((int)val1).CompareTo(val2);
        
            if(val1 is float)
                return ((float)val1).CompareTo((int)val2);
         
            return ((float)(int)val1).CompareTo(val2);
        }
    }
}