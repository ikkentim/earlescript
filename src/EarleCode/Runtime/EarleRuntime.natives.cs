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
using EarleCode.Runtime.Events;
using EarleCode.Runtime.Values;

namespace EarleCode.Runtime
{
    public partial class EarleRuntime
    {
        private void RegisterDefaultNatives()
        {
            RegisterNativesInType<EarleDefaultNatives>();
            RegisterNativesInType<EarleEventManagerNatives>();

            RegisterNative(CreateBinaryOperator("*", (left, right) => (EarleValue)(left.Is<float>() || right.Is<float>()
                                                      ? (float)left*(float)right
                                                                                   : (int)left*(int)right), typeof (int), typeof (float)));

            RegisterNative(CreateBinaryOperator("+", (left, right) =>
            {
                if (left.Is<string>() || right.Is<string>())
                    return (EarleValue)((string)left + (string)right);
                if (left.Is<float>() && right.Is<float>())
                    return (EarleValue)((float)left + (float)right);
                if (left.Is<int>() && right.Is<float>())
                    return (EarleValue)((int)left + (float)right);
                if (left.Is<float>() && right.Is<int>())
                    return (EarleValue)((float)left + (int)right);
                if(left.Is<int>() && right.Is<int>())
                    return (EarleValue)((int)left + (int)right);

                return EarleValue.Undefined;
            }));

            RegisterNative(CreateBinaryOperator("-", (left, right) => (EarleValue)(left.Is<float>() || right.Is<float>()
                                                      ? (float)left - (float)right
                                                                                   : (int)left - (int)right), typeof (int), typeof (float), typeof (string)));

            RegisterNative(CreateBinaryOperator("<", (left, right) => (EarleValue)(CompareFloatInt(left.Value, right.Value) < 0), 
                                                      typeof (int), typeof (float)));

            RegisterNative(CreateBinaryOperator("<=", (left, right) => (EarleValue)(CompareFloatInt(left.Value, right.Value) <= 0), 
                                                      typeof (int), typeof (float)));

            RegisterNative(CreateBinaryOperator(">", (left, right) => (EarleValue)(CompareFloatInt(left.Value, right.Value) > 0),
                                                      typeof(int), typeof(float)));

            RegisterNative(CreateBinaryOperator(">=", (left, right) => (EarleValue)(CompareFloatInt(left.Value, right.Value) >= 0),
                                                      typeof(int), typeof(float)));

            RegisterNative(CreateBinaryOperator("==", (left, right) => {
                if((left.Is<int>() || left.Is<float>()) && (right.Is<int>() || right.Is<float>()))
                    return (EarleValue)(CompareFloatInt(left.Value, right.Value) == 0);


                return (EarleValue)(left.Value == null
                    ? right.Value == null
                                    : left.Value.Equals(right.Value));
            }));

            RegisterNative(CreateBinaryOperator("!=", (left, right) => {
                if((left.Is<int>() || left.Is<float>()) && (right.Is<int>() || right.Is<float>()))
                    return (EarleValue)(CompareFloatInt(left.Value, right.Value) != 0);


                return (EarleValue)(left.Value == null
                    ? right.Value != null
                                    : !left.Value.Equals(right.Value));
            }));

            RegisterNative(CreateUnaryOperator("++",
                                                     v => v.Is<int>() ? ((int)v + 1).ToEarleValue() : ((float)v+ 1).ToEarleValue(), typeof (int),
                typeof (float)));

            RegisterNative(CreateUnaryOperator("--",
                v => v.Is<int>() ? (v.As<int>() - 1).ToEarleValue() : (v.As<float>() - 1).ToEarleValue(), typeof (int),
                typeof (float)));
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

        private EarleFunction CreateBinaryOperator(string @operator, Func<EarleValue, EarleValue, EarleValue> operation,
                                                   params Type[] supportedTypes)
        {
            if(@operator == null) throw new ArgumentNullException(nameof(@operator));
            if(operation == null) throw new ArgumentNullException(nameof(operation));
            if(supportedTypes == null) throw new ArgumentNullException(nameof(supportedTypes));

            return EarleNativeFunction.Create($"operator{@operator}", (EarleValue left, EarleValue right) => {
                return supportedTypes.Length > 0 && (!left.IsAny(supportedTypes) || !right.IsAny(supportedTypes))
                    ? EarleValue.Undefined
                    : operation(left, right);
            });
        }

        private EarleFunction CreateUnaryOperator(string @operator, Func<EarleValue, EarleValue> operation,
                                                   params Type[] supportedTypes)
        {
            if(@operator == null) throw new ArgumentNullException(nameof(@operator));
            if(operation == null) throw new ArgumentNullException(nameof(operation));
            if(supportedTypes == null) throw new ArgumentNullException(nameof(supportedTypes));

            return EarleNativeFunction.Create($"operator{@operator}", (EarleValue value) => {
                return supportedTypes.Length > 0 && !value.IsAny(supportedTypes)
                    ? EarleValue.Undefined
                    : operation(value);
            });
        }
    }
}