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
using System.Linq;
using EarleCode.Values;
using EarleCode.Values.ValueTypes;

namespace EarleCode
{
    public partial class Runtime
    {
        private void RegisterDefaultNatives()
        {
            RegisterNative(new NativeFunction("print", values =>
            {
                Console.WriteLine(values.FirstOrDefault().Value);
                return EarleValue.Null;
            }, "value"));

            RegisterNative(new BinaryOperatorNativeFunction("*", (left, right) =>
            {
                return left.Is<float>() || right.Is<float>()
                    ? new EarleValue(left.To<float>(this)*right.To<float>(this))
                    : new EarleValue(left.To<int>(this)*right.To<int>(this));
            }, typeof (int), typeof (float)));

            RegisterNative(new BinaryOperatorNativeFunction("+", (left, right) =>
            {
                if (left.Is<string>() || right.Is<string>())
                    return (new EarleValue(left.To<string>(this) + right.To<string>(this)));
                if (left.Is<float>() || right.Is<float>())
                    return (new EarleValue(left.To<float>(this) + right.To<float>(this)));
                return (new EarleValue(left.To<int>(this) + right.To<int>(this)));
            }, typeof (int), typeof (float), typeof (string)));

            RegisterNative(new BinaryOperatorNativeFunction("-", (left, right) =>
            {
                return left.Is<float>() || right.Is<float>()
                    ? new EarleValue(left.To<float>(this) - right.To<float>(this))
                    : new EarleValue(left.To<int>(this) - right.To<int>(this));
            }, typeof (int), typeof (float), typeof (string)));

            RegisterNative(new BinaryOperatorNativeFunction("<", (left, right) =>
            {
                if (left.Is<int>() && right.Is<int>())
                    return left.As<int>() < right.As<int>() ? EarleValue.True : EarleValue.False;
                if (left.Is<int>() && right.Is<float>())
                    return left.As<int>() < right.As<float>() ? EarleValue.True : EarleValue.False;
                if (left.Is<float>() && right.Is<int>())
                    return left.As<float>() < right.As<int>() ? EarleValue.True : EarleValue.False;
                if (left.Is<float>() && right.Is<float>())
                    return left.As<float>() < right.As<float>() ? EarleValue.True : EarleValue.False;

                throw new Exception("Unsupported value type");
            }, typeof (int), typeof (float)));

            RegisterNative(new BinaryOperatorNativeFunction("<=", (left, right) =>
            {
                if (left.Is<int>() && right.Is<int>())
                    return left.As<int>() <= right.As<int>() ? EarleValue.True : EarleValue.False;
                if (left.Is<int>() && right.Is<float>())
                    return left.As<int>() <= right.As<float>() ? EarleValue.True : EarleValue.False;
                if (left.Is<float>() && right.Is<int>())
                    return left.As<float>() <= right.As<int>() ? EarleValue.True : EarleValue.False;
                if (left.Is<float>() && right.Is<float>())
                    return left.As<float>() <= right.As<float>() ? EarleValue.True : EarleValue.False;

                throw new Exception("Unsupported value type");
            }, typeof (int), typeof (float)));

            RegisterNative(new BinaryOperatorNativeFunction(">", (left, right) =>
            {
                if (left.Is<int>() && right.Is<int>())
                    return left.As<int>() > right.As<int>() ? EarleValue.True : EarleValue.False;
                if (left.Is<int>() && right.Is<float>())
                    return left.As<int>() > right.As<float>() ? EarleValue.True : EarleValue.False;
                if (left.Is<float>() && right.Is<int>())
                    return left.As<float>() > right.As<int>() ? EarleValue.True : EarleValue.False;
                if (left.Is<float>() && right.Is<float>())
                    return left.As<float>() > right.As<float>() ? EarleValue.True : EarleValue.False;

                throw new Exception("Unsupported value type");
            }, typeof (int), typeof (float)));

            RegisterNative(new BinaryOperatorNativeFunction(">=", (left, right) =>
            {
                if (left.Is<int>() && right.Is<int>())
                    return left.As<int>() >= right.As<int>() ? EarleValue.True : EarleValue.False;
                if (left.Is<int>() && right.Is<float>())
                    return left.As<int>() >= right.As<float>() ? EarleValue.True : EarleValue.False;
                if (left.Is<float>() && right.Is<int>())
                    return left.As<float>() >= right.As<int>() ? EarleValue.True : EarleValue.False;
                if (left.Is<float>() && right.Is<float>())
                    return left.As<float>() >= right.As<float>() ? EarleValue.True : EarleValue.False;

                throw new Exception("Unsupported value type");
            }, typeof (int), typeof (float)));

            RegisterNative(new BinaryOperatorNativeFunction("==", (left, right) =>
            {
                if (left.Is<int>() && right.Is<float>())
                    return left.As<int>() == right.As<float>() ? EarleValue.True : EarleValue.False;
                if (left.Is<float>() && right.Is<int>())
                    return left.As<float>() == right.As<int>() ? EarleValue.True : EarleValue.False;

                return left.Is(null)
                    ? (right.Is(null) ? EarleValue.True : EarleValue.False)
                    : (left.Value.Equals(right.Value) ? EarleValue.True : EarleValue.False);
            }));

            RegisterNative(new BinaryOperatorNativeFunction("!=", (left, right) =>
            {
                if (left.Is<int>() && right.Is<float>())
                    return left.As<int>() != right.As<float>() ? EarleValue.True : EarleValue.False;
                if (left.Is<float>() && right.Is<int>())
                    return left.As<float>() != right.As<int>() ? EarleValue.True : EarleValue.False;

                return left.Is(null)
                    ? (!right.Is(null) ? EarleValue.True : EarleValue.False)
                    : (!left.Value.Equals(right.Value) ? EarleValue.True : EarleValue.False);
            }));

            RegisterNative(new UnaryOperatorNativeFunction("++",
                v => v.Is<int>() ? (v.As<int>() + 1).ToEarleValue() : (v.As<float>() + 1).ToEarleValue(), typeof(int),
                typeof(float)));

            RegisterNative(new UnaryOperatorNativeFunction("--",
                v => v.Is<int>() ? (v.As<int>() - 1).ToEarleValue() : (v.As<float>() - 1).ToEarleValue(), typeof(int),
                typeof(float)));
        }

        private void RegisterDefaultValueTypes()
        {
            RegisterValueType(new EarleIntegerValueType());
            RegisterValueType(new EarleFloatValueType());
            RegisterValueType(new EarleBoolValueType());
            RegisterValueType(new EarleStringValueType());
        }

        private class BinaryOperatorNativeFunction : NativeFunction
        {
            public BinaryOperatorNativeFunction(string @operator, Func<EarleValue, EarleValue, EarleValue> operation,
                params Type[] supportedTypes)
                : base($"operator{@operator}", values =>
                {
                    var left = values[0];
                    var right = values[1];

                    if (supportedTypes.Length > 0)
                    {
                        left.AssertOfType(supportedTypes);
                        right.AssertOfType(supportedTypes);
                    }

                    return operation(left, right);
                }, "left", "right")
            {
                if (@operator == null) throw new ArgumentNullException(nameof(@operator));
                if (operation == null) throw new ArgumentNullException(nameof(operation));
                if (supportedTypes == null) throw new ArgumentNullException(nameof(supportedTypes));
            }
        }
        private class UnaryOperatorNativeFunction : NativeFunction
        {
            public UnaryOperatorNativeFunction(string @operator, Func<EarleValue, EarleValue> operation,
                params Type[] supportedTypes)
                : base($"operator{@operator}", values =>
                {
                    var value = values[0];

                    if (supportedTypes.Length > 0)
                        value.AssertOfType(supportedTypes);
                    
                    return operation(value);
                }, "value")
            {
                if (@operator == null) throw new ArgumentNullException(nameof(@operator));
                if (operation == null) throw new ArgumentNullException(nameof(operation));
                if (supportedTypes == null) throw new ArgumentNullException(nameof(supportedTypes));
            }
        }
    }
}