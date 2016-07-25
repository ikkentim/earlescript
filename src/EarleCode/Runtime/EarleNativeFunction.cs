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
using System.Reflection;
using EarleCode.Runtime.Values;

namespace EarleCode.Runtime
{
    public abstract class EarleNativeFunction : EarleFunction
    {
        protected EarleNativeFunction(string name, params string[] parameters) : base(null, name, parameters, null)
        {
        }

        #region Overrides of EarleFunction

        public override EarleStackFrameExecutor CreateFrameExecutor(EarleStackFrame superFrame, EarleValue target, EarleValue[] arguments)
        {
            if (Parameters.Length < arguments.Length)
                arguments =
                    arguments.Concat(Enumerable.Repeat(EarleValue.Undefined, Parameters.Length - arguments.Length))
                        .ToArray();

            return new NativeStackFrameExecutor(new EarleStackFrame(superFrame.Runtime, target), this, arguments);
        }

        #endregion

        protected abstract EarleValue Invoke(EarleStackFrame frame, EarleValue[] arguments);

        private class NativeStackFrameExecutor : EarleStackFrameExecutor
        {
            private readonly EarleValue[] _arguments;
            private readonly EarleNativeFunction _native;

            public NativeStackFrameExecutor(EarleStackFrame frame, EarleNativeFunction native, EarleValue[] arguments)
                : base(frame, null, null)
            {
                _native = native;
                _arguments = arguments;
            }

            #region Overrides of RuntimeLoop

            public override EarleValue? Run()
            {
                if(Frame.SubFrame != null)
                {
                    var result = Frame.SubFrame.Run();

                    if(result != null)
                        Frame.SubFrame = null;

                    return result;
                }
                else
                {
                    var result = _native.Invoke(Frame, _arguments);

                    if(Frame.SubFrame == null)
                        return result;

                    return null;
                }
            }

            #endregion
        }

        public static EarleNativeFunction Create(string name, object target, MethodInfo methodInfo)
        {
            var parameters = methodInfo.GetParameters();
            var passStackFrame = false;

            for(var i = 0; i < parameters.Length; i++)
            {
                if(i == 0 && parameters[i].ParameterType == typeof(EarleStackFrame))
                {
                    passStackFrame = true;
                }
                else if(parameters[i].ParameterType != typeof(EarleValue))
                {
                    throw new ArgumentException("Method may only contain EarleValue arguments.", nameof(methodInfo));
                }
            }

            if(methodInfo.ReturnType != typeof(void) && methodInfo.ReturnType != typeof(EarleValue))
            {
                throw new ArgumentException("Method may only return EarleValue or void.", nameof(methodInfo));
            }

            return new NativeFunction(name, target, methodInfo, passStackFrame);
        }

        public static EarleNativeFunction Create(string name, Action action) => Create(name, null, action.Method);
        public static EarleNativeFunction Create<T1>(string name, Action<T1> action) => Create(name, null, action.Method);
        public static EarleNativeFunction Create<T1, T2>(string name, Action<T1, T2> action) => Create(name, null, action.Method);
        public static EarleNativeFunction Create<T1, T2, T3>(string name, Action<T1, T2, T3> action) => Create(name, null, action.Method);
        public static EarleNativeFunction Create<T1, T2, T3, T4>(string name, Action<T1, T2, T3, T4> action) => Create(name, null, action.Method);
        public static EarleNativeFunction Create<T1, T2, T3, T4, T5>(string name, Action<T1, T2, T3, T4, T5> action) => Create(name, null, action.Method);
        public static EarleNativeFunction Create<T1, T2, T3, T4, T5, T6>(string name, Action<T1, T2, T3, T4, T5, T6> action) => Create(name, null, action.Method);
        public static EarleNativeFunction Create<T1, T2, T3, T4, T5, T6, T7>(string name, Action<T1, T2, T3, T4, T5, T6, T7> action) => Create(name, null, action.Method);
        public static EarleNativeFunction Create<T1, T2, T3, T4, T5, T6, T7, T8>(string name, Action<T1, T2, T3, T4, T5, T6, T7, T8> action) => Create(name, null, action.Method);
        public static EarleNativeFunction Create<T1, T2, T3, T4, T5, T6, T7, T8, T9>(string name, Action<T1, T2, T3, T4, T5, T6, T7, T8, T9> action) => Create(name, null, action.Method);
        public static EarleNativeFunction Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(string name, Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> action) => Create(name, null, action.Method);
        public static EarleNativeFunction Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(string name, Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> action) => Create(name, null, action.Method);
        public static EarleNativeFunction Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(string name, Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> action) => Create(name, null, action.Method);
        public static EarleNativeFunction Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(string name, Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> action) => Create(name, null, action.Method);
        public static EarleNativeFunction Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(string name, Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> action) => Create(name, null, action.Method);
        public static EarleNativeFunction Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(string name, Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> action) => Create(name, null, action.Method);
        public static EarleNativeFunction Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(string name, Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> action) => Create(name, null, action.Method);

        public static EarleNativeFunction Create<TResult>(string name, Func<TResult> func) => Create(name, null, func.Method);
        public static EarleNativeFunction Create<T1, TResult>(string name, Func<T1, TResult> func) => Create(name, null, func.Method);
        public static EarleNativeFunction Create<T1, T2, TResult>(string name, Func<T1, T2, TResult> func) => Create(name, null, func.Method);
        public static EarleNativeFunction Create<T1, T2, T3, TResult>(string name, Func<T1, T2, T3, TResult> func) => Create(name, null, func.Method);
        public static EarleNativeFunction Create<T1, T2, T3, T4, TResult>(string name, Func<T1, T2, T3, T4, TResult> func) => Create(name, null, func.Method);
        public static EarleNativeFunction Create<T1, T2, T3, T4, T5, TResult>(string name, Func<T1, T2, T3, T4, T5, TResult> func) => Create(name, null, func.Method);
        public static EarleNativeFunction Create<T1, T2, T3, T4, T5, T6, TResult>(string name, Func<T1, T2, T3, T4, T5, T6, TResult> func) => Create(name, null, func.Method);
        public static EarleNativeFunction Create<T1, T2, T3, T4, T5, T6, T7, TResult>(string name, Func<T1, T2, T3, T4, T5, T6, T7, TResult> func) => Create(name, null, func.Method);
        public static EarleNativeFunction Create<T1, T2, T3, T4, T5, T6, T7, T8, TResult>(string name, Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> func) => Create(name, null, func.Method);
        public static EarleNativeFunction Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>(string name, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> func) => Create(name, null, func.Method);
        public static EarleNativeFunction Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>(string name, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult> func) => Create(name, null, func.Method);
        public static EarleNativeFunction Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult>(string name, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult> func) => Create(name, null, func.Method);
        public static EarleNativeFunction Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult>(string name, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult> func) => Create(name, null, func.Method);
        public static EarleNativeFunction Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult>(string name, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult> func) => Create(name, null, func.Method);
        public static EarleNativeFunction Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult>(string name, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult> func) => Create(name, null, func.Method);
        public static EarleNativeFunction Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult>(string name, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult> func) => Create(name, null, func.Method);
        public static EarleNativeFunction Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult>(string name, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult> func) => Create(name, null, func.Method);

        private class NativeFunction : EarleNativeFunction
        {
            private readonly object _target;
            private readonly MethodInfo _methodInfo;
            private readonly bool _passStackFrame;

            public NativeFunction(string name, object target, MethodInfo methodInfo, bool passStackFrame) : 
            base(name, methodInfo.GetParameters().Skip(passStackFrame ? 1 : 0).Select(p => p.Name).ToArray())
            {
                if(methodInfo == null) throw new ArgumentNullException(nameof(methodInfo));

                _target = target;
                _methodInfo = methodInfo;
                _passStackFrame = passStackFrame;
            }

            protected override EarleValue Invoke(EarleStackFrame frame, EarleValue[] arguments)
            {
                var result = _passStackFrame
                    ? _methodInfo.Invoke(_target, new object[] { frame }.Concat(arguments.Cast<object>()).ToArray())
                    : _methodInfo.Invoke(_target, arguments.Cast<object>().ToArray());

                if(result is EarleValue)
                    return (EarleValue)result;
                return EarleValue.Undefined;
            }
        }
    }
}