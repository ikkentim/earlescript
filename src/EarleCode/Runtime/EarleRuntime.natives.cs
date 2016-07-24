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
using EarleCode.Runtime.Values;
using EarleCode.Runtime.Values.ValueTypes;

namespace EarleCode.Runtime
{
    public partial class EarleRuntime
    {
        private void RegisterDefaultNatives()
        {
            RegisterNative(new WaitFunction());
            RegisterNative(new EarleInlineNativeFunction("print", values =>
            {
                Console.WriteLine(values.FirstOrDefault().Value);
                return EarleValue.Undefined;
            }, "value"));

            RegisterNative(new EarleInlineNativeFunction("createvector2",
                values => new EarleVector2(
                    values[0].Is<float>() ? values[0].As<float>() : values[0].As<int>(),
                    values[1].Is<float>() ? values[1].As<float>() : values[1].As<int>()
                    ).ToEarleValue()
                , "x", "y"));

            RegisterNative(new EarleInlineNativeFunction("createvector3",
                values =>
                    new EarleVector3(
                        values[0].Is<float>() ? values[0].As<float>() : values[0].As<int>(),
                        values[1].Is<float>() ? values[1].As<float>() : values[1].As<int>(),
                        values[2].Is<float>() ? values[2].As<float>() : values[2].As<int>()
                        ).ToEarleValue(),
                "x", "y", "z"));

            RegisterNative(new EarleInlineNativeFunction("spawnstruct",
                values => new EarleStructure().ToEarleValue()));

            RegisterNative(new BinaryOperatorFunction("*", (left, right) => left.Is<float>() || right.Is<float>()
                ? new EarleValue(left.To<float>(this)*right.To<float>(this))
                : new EarleValue(left.To<int>(this)*right.To<int>(this)), typeof (int), typeof (float)));

            RegisterNative(new BinaryOperatorFunction("+", (left, right) =>
            {
                if (left.Is<string>() || right.Is<string>())
                    return (new EarleValue(left.To<string>(this) + right.To<string>(this)));
                if (left.Is<float>() && right.Is<float>())
                    return (new EarleValue(left.To<float>(this) + right.To<float>(this)));
                if (left.Is<int>() && right.Is<float>())
                    return (new EarleValue(left.To<int>(this) + right.To<float>(this)));
                if (left.Is<float>() && right.Is<int>())
                    return (new EarleValue(left.To<float>(this) + right.To<int>(this)));
                if (left.Is<int>() && right.Is<int>())
                    return (new EarleValue(left.To<int>(this) + right.To<int>(this)));

                return EarleValue.Undefined;
            }));

            RegisterNative(new BinaryOperatorFunction("-", (left, right) => left.Is<float>() || right.Is<float>()
                ? new EarleValue(left.To<float>(this) - right.To<float>(this))
                : new EarleValue(left.To<int>(this) - right.To<int>(this)), typeof (int), typeof (float),
                typeof (string)));

            RegisterNative(new BinaryBooleanOperatorFunction("<", (left, right) =>
            {
                if (left.Is<int>() && right.Is<int>())
                    return left.As<int>() < right.As<int>();
                if (left.Is<int>() && right.Is<float>())
                    return left.As<int>() < right.As<float>();
                if (left.Is<float>() && right.Is<int>())
                    return left.As<float>() < right.As<int>();
                if (left.Is<float>() && right.Is<float>())
                    return left.As<float>() < right.As<float>();

                return false;
            }, typeof (int), typeof (float)));

            RegisterNative(new BinaryBooleanOperatorFunction("<=", (left, right) =>
            {
                if (left.Is<int>() && right.Is<int>())
                    return left.As<int>() <= right.As<int>();
                if (left.Is<int>() && right.Is<float>())
                    return left.As<int>() <= right.As<float>();
                if (left.Is<float>() && right.Is<int>())
                    return left.As<float>() <= right.As<int>();
                if (left.Is<float>() && right.Is<float>())
                    return left.As<float>() <= right.As<float>();

                return false;
            }, typeof (int), typeof (float)));

            RegisterNative(new BinaryBooleanOperatorFunction(">", (left, right) =>
            {
                if (left.Is<int>() && right.Is<int>())
                    return left.As<int>() > right.As<int>();
                if (left.Is<int>() && right.Is<float>())
                    return left.As<int>() > right.As<float>();
                if (left.Is<float>() && right.Is<int>())
                    return left.As<float>() > right.As<int>();
                if (left.Is<float>() && right.Is<float>())
                    return left.As<float>() > right.As<float>();

                return false;
            }, typeof (int), typeof (float)));

            RegisterNative(new BinaryBooleanOperatorFunction(">=", (left, right) =>
            {
                if (left.Is<int>() && right.Is<int>())
                    return left.As<int>() >= right.As<int>();
                if (left.Is<int>() && right.Is<float>())
                    return left.As<int>() >= right.As<float>();
                if (left.Is<float>() && right.Is<int>())
                    return left.As<float>() >= right.As<int>();
                if (left.Is<float>() && right.Is<float>())
                    return left.As<float>() >= right.As<float>();

                return false;
            }, typeof (int), typeof (float)));

            RegisterNative(new BinaryBooleanOperatorFunction("==", (left, right) =>
            {
                if (left.Is<int>() && right.Is<float>())
                    return left.As<int>() == right.As<float>();
                if (left.Is<float>() && right.Is<int>())
                    return left.As<float>() == right.As<int>();

                return left.Is(null)
                    ? right.Is(null)
                    : left.Value.Equals(right.Value);
            }));

            RegisterNative(new BinaryBooleanOperatorFunction("!=", (left, right) =>
            {
                if (left.Is<int>() && right.Is<float>())
                    return left.As<int>() != right.As<float>();
                if (left.Is<float>() && right.Is<int>())
                    return left.As<float>() != right.As<int>();

                return left.Is(null)
                    ? !right.Is(null)
                    : !left.Value.Equals(right.Value);
            }));

            RegisterNative(new UnaryOperatorFunction("++",
                v => v.Is<int>() ? (v.As<int>() + 1).ToEarleValue() : (v.As<float>() + 1).ToEarleValue(), typeof (int),
                typeof (float)));

            RegisterNative(new UnaryOperatorFunction("--",
                v => v.Is<int>() ? (v.As<int>() - 1).ToEarleValue() : (v.As<float>() - 1).ToEarleValue(), typeof (int),
                typeof (float)));
        }

        private void RegisterDefaultValueTypes()
        {
            RegisterValueType(new EarleIntegerValueType());
            RegisterValueType(new EarleFloatValueType());
            RegisterValueType(new EarleBoolValueType());
            RegisterValueType(new EarleStringValueType());
        }

        private class BinaryBooleanOperatorFunction : BinaryOperatorFunction
        {
            public BinaryBooleanOperatorFunction(string @operator, Func<EarleValue, EarleValue, bool> operation,
                params Type[] supportedTypes)
                : base(@operator, (l, r) => operation(l, r) ? EarleValue.True : EarleValue.False, supportedTypes)
            {
            }
        }

        private class WaitFunction : EarleFunction
        {
            public WaitFunction() : base(null, "wait", new[] { "seconds" }, null)
            {
            }

            public override EarleRuntimeLoop CreateLoop(EarleRuntime runtime, EarleValue[] arguments, EarleValue target)
            {
                var seconds = arguments.Length >= 1 ? arguments[0].To<float>(runtime) : 0;
                return new WaitLoop(runtime, seconds);
            }

            private class WaitLoop : EarleRuntimeLoop
            {
                private Stopwatch _stopwatch;
                private long _miliseconds;

                public WaitLoop(EarleRuntime runtime, float seconds) : base(runtime, null, null, EarleValue.Undefined)
                {
                    _stopwatch = new Stopwatch();
                    _stopwatch.Start();
                    _miliseconds = (long)(seconds * 1000);
                }

                public override EarleValue? Run()
                {
                    return _stopwatch.ElapsedMilliseconds >= _miliseconds ? (EarleValue?)EarleValue.Undefined : null;
                }
            }
        }
        private class BinaryOperatorFunction : EarleInlineNativeFunction
        {
            public BinaryOperatorFunction(string @operator, Func<EarleValue, EarleValue, EarleValue> operation,
                params Type[] supportedTypes)
                : base($"operator{@operator}", values =>
                {
                    var left = values[0];
                    var right = values[1];

                    return supportedTypes.Length > 0 && (!left.IsAny(supportedTypes) || !right.IsAny(supportedTypes))
                        ? EarleValue.Undefined
                        : operation(left, right);
                }, "left", "right")
            {
                if (@operator == null) throw new ArgumentNullException(nameof(@operator));
                if (operation == null) throw new ArgumentNullException(nameof(operation));
                if (supportedTypes == null) throw new ArgumentNullException(nameof(supportedTypes));
            }
        }

        private class UnaryOperatorFunction : EarleInlineNativeFunction
        {
            public UnaryOperatorFunction(string @operator, Func<EarleValue, EarleValue> operation,
                params Type[] supportedTypes)
                : base($"operator{@operator}", values =>
                {
                    var value = values[0];

                    return supportedTypes.Length > 0 && !value.IsAny(supportedTypes)
                        ? EarleValue.Undefined
                        : operation(value);
                }, "value")
            {
                if (@operator == null) throw new ArgumentNullException(nameof(@operator));
                if (operation == null) throw new ArgumentNullException(nameof(operation));
                if (supportedTypes == null) throw new ArgumentNullException(nameof(supportedTypes));
            }
        }
    }
}