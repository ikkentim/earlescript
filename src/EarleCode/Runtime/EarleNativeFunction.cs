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
        protected EarleNativeFunction(string name, params string[] parameters)
            : base(null, name, parameters, null, null)
        {
        }

        #region Overrides of EarleFunction

        public override IEarleStackFrameExecutor CreateFrameExecutor(EarleStackFrame parentFrame, int callerIp,
            EarleValue target, EarleValue[] arguments)
        {
            if (Parameters != null && Parameters.Length < arguments.Length)
                arguments =
                    arguments.Concat(Enumerable.Repeat(EarleValue.Undefined, Parameters.Length - arguments.Length))
                        .Take(Parameters.Length)
                        .ToArray();

            return new NativeStackFrameExecutor(this, parentFrame, callerIp, target, arguments);
        }

        #endregion

        protected abstract EarleValue Invoke(EarleStackFrame frame, EarleValue[] arguments);

        public static EarleNativeFunction Create(string name, object target, MethodInfo methodInfo)
        {
            var parameters = methodInfo.GetParameters();
            var passStackFrame = false;
            var varArgs = false;

            for (var i = 0; i < parameters.Length; i++)
            {
                if (i == 0 && parameters[i].ParameterType == typeof (EarleStackFrame))
                {
                    passStackFrame = true;
                }
                else if (parameters[i].ParameterType == typeof (EarleValue[]))
                {
                    if (i != parameters.Length - 1)
                    {
                        throw new ArgumentException("Method may only contain EarleValue[] argument as last argument.",
                            nameof(methodInfo));
                    }

                    varArgs = true;
                }
                else if (parameters[i].ParameterType != typeof (EarleValue) &&
                         !EarleValueTypeStore.IsSupportedType(parameters[i].ParameterType))
                {
                    throw new ArgumentException("Method may only contain EarleValue arguments.", nameof(methodInfo));
                }
            }

            if (methodInfo.ReturnType != typeof (void) && methodInfo.ReturnType != typeof (EarleValue) &&
                !EarleValueTypeStore.IsSupportedType(methodInfo.ReturnType))
            {
                throw new ArgumentException("Method may only return EarleValue or void.", nameof(methodInfo));
            }

            var paramNames = varArgs
                ? null
                : methodInfo.GetParameters().Skip(passStackFrame ? 1 : 0).Select(p => p.Name).ToArray();
            return new LambdaFunction(name, target, methodInfo, paramNames);
        }

        public static EarleNativeFunction Create(string name, Action action)
            => Create(name, action.Target, action.Method);

        public static EarleNativeFunction Create<T1>(string name, Action<T1> action)
            => Create(name, action.Target, action.Method);

        public static EarleNativeFunction Create<T1, T2>(string name, Action<T1, T2> action)
            => Create(name, action.Target, action.Method);

        public static EarleNativeFunction Create<T1, T2, T3>(string name, Action<T1, T2, T3> action)
            => Create(name, action.Target, action.Method);

        public static EarleNativeFunction Create<T1, T2, T3, T4>(string name, Action<T1, T2, T3, T4> action)
            => Create(name, action.Target, action.Method);

        public static EarleNativeFunction Create<T1, T2, T3, T4, T5>(string name, Action<T1, T2, T3, T4, T5> action)
            => Create(name, action.Target, action.Method);

        public static EarleNativeFunction Create<T1, T2, T3, T4, T5, T6>(string name,
            Action<T1, T2, T3, T4, T5, T6> action)
            => Create(name, action.Target, action.Method);

        public static EarleNativeFunction Create<T1, T2, T3, T4, T5, T6, T7>(string name,
            Action<T1, T2, T3, T4, T5, T6, T7> action) => Create(name, action.Target, action.Method);

        public static EarleNativeFunction Create<T1, T2, T3, T4, T5, T6, T7, T8>(string name,
            Action<T1, T2, T3, T4, T5, T6, T7, T8> action) => Create(name, action.Target, action.Method);

        public static EarleNativeFunction Create<T1, T2, T3, T4, T5, T6, T7, T8, T9>(string name,
            Action<T1, T2, T3, T4, T5, T6, T7, T8, T9> action) => Create(name, action.Target, action.Method);

        public static EarleNativeFunction Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(string name,
            Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> action) => Create(name, action.Target, action.Method);

        public static EarleNativeFunction Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(string name,
            Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> action) => Create(name, action.Target, action.Method);

        public static EarleNativeFunction Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(string name,
            Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> action)
            => Create(name, action.Target, action.Method);

        public static EarleNativeFunction Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(string name,
            Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> action)
            => Create(name, action.Target, action.Method);

        public static EarleNativeFunction Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(
            string name,
            Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> action)
            => Create(name, action.Target, action.Method);

        public static EarleNativeFunction Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(
            string name, Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> action)
            => Create(name, action.Target, action.Method);

        public static EarleNativeFunction Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(
            string name, Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> action)
            => Create(name, action.Target, action.Method);

        public static EarleNativeFunction Create<TResult>(string name, Func<TResult> func)
            => Create(name, func.Target, func.Method);

        public static EarleNativeFunction Create<T1, TResult>(string name, Func<T1, TResult> func)
            => Create(name, func.Target, func.Method);

        public static EarleNativeFunction Create<T1, T2, TResult>(string name, Func<T1, T2, TResult> func)
            => Create(name, func.Target, func.Method);

        public static EarleNativeFunction Create<T1, T2, T3, TResult>(string name, Func<T1, T2, T3, TResult> func)
            => Create(name, func.Target, func.Method);

        public static EarleNativeFunction Create<T1, T2, T3, T4, TResult>(string name,
            Func<T1, T2, T3, T4, TResult> func)
            => Create(name, func.Target, func.Method);

        public static EarleNativeFunction Create<T1, T2, T3, T4, T5, TResult>(string name,
            Func<T1, T2, T3, T4, T5, TResult> func) => Create(name, func.Target, func.Method);

        public static EarleNativeFunction Create<T1, T2, T3, T4, T5, T6, TResult>(string name,
            Func<T1, T2, T3, T4, T5, T6, TResult> func) => Create(name, func.Target, func.Method);

        public static EarleNativeFunction Create<T1, T2, T3, T4, T5, T6, T7, TResult>(string name,
            Func<T1, T2, T3, T4, T5, T6, T7, TResult> func) => Create(name, func.Target, func.Method);

        public static EarleNativeFunction Create<T1, T2, T3, T4, T5, T6, T7, T8, TResult>(string name,
            Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> func) => Create(name, func.Target, func.Method);

        public static EarleNativeFunction Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>(string name,
            Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> func) => Create(name, func.Target, func.Method);

        public static EarleNativeFunction Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>(string name,
            Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult> func) => Create(name, func.Target, func.Method);

        public static EarleNativeFunction Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult>(string name,
            Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult> func) => Create(name, func.Target, func.Method);

        public static EarleNativeFunction Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult>(
            string name,
            Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult> func)
            => Create(name, func.Target, func.Method);

        public static EarleNativeFunction Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult>(
            string name,
            Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult> func)
            => Create(name, func.Target, func.Method);

        public static EarleNativeFunction Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult>(
            string name, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult> func)
            => Create(name, func.Target, func.Method);

        public static EarleNativeFunction Create
            <T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult>(
            string name, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult> func)
            => Create(name, func.Target, func.Method);

        public static EarleNativeFunction Create
            <T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult>(string name,
                Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult> func)
            => Create(name, func.Target, func.Method);

        private class NativeStackFrameExecutor : EarleBaseStackFrameExecutor
        {
            private readonly EarleValue[] _arguments;
            private readonly EarleNativeFunction _native;

            public NativeStackFrameExecutor(EarleNativeFunction native, EarleStackFrame parentFrame, int callerIp,
                EarleValue target, EarleValue[] arguments) : base(target)
            {
                if (arguments == null)
                    throw new ArgumentNullException(nameof(arguments));
                Frame = parentFrame.SpawnChild(native, this, callerIp);

                _native = native;
                _arguments = arguments;
            }

            #region Overrides of RuntimeLoop

            public override EarleValue? Run()
            {
                if (Frame.ChildFrame != null)
                {
                    var result = Frame.ChildFrame.Executor.Run();

                    if (result != null)
                        Frame.ChildFrame = null;

                    return result;
                }
                else
                {
                    var result = _native.Invoke(Frame, _arguments);

                    if (Frame.ChildFrame == null)
                        return result;

                    return null;
                }
            }

            #endregion
        }

	    public static EarleValue Invoke(string name, MethodInfo methodInfo, object target, EarleStackFrame frame, EarleValue[] arguments)
	    {
			var parameters = methodInfo.GetParameters();
			var args = new object[parameters.Length];

			for (int i = 0, a = 0; i < args.Length; i++)
			{
				var parameter = parameters[i];

				if (parameter.ParameterType == typeof(EarleValue[]))
				{
					args[i] = a >= arguments.Length ? new EarleValue[0] : arguments.Skip(a++).ToArray();
					break;
				}

				if (i == 0 && parameter.ParameterType == typeof(EarleStackFrame))
				{
					args[i] = frame;
					continue;
				}

				if (a >= arguments.Length)
				{
					frame.Runtime.HandleWarning($"Out of arguments while invoking native '{name}'");
					continue;
				}

				if (parameter.ParameterType == typeof(EarleValue))
				{
					args[i] = arguments[a++];
				}
				else
				{
					args[i] = arguments[a++].CastTo(parameter.ParameterType);
				}
			}

			var result = methodInfo.Invoke(target, args);

			if (result is EarleValue)
				return (EarleValue)result;
			return new EarleValue(result);
		}

        private class LambdaFunction : EarleNativeFunction
        {
            private readonly MethodInfo _methodInfo;
            private readonly object _target;

            public LambdaFunction(string name, object target, MethodInfo methodInfo, string[] paramNames) :
                base(name, paramNames)
            {
                if (methodInfo == null) throw new ArgumentNullException(nameof(methodInfo));

                _target = target;
                _methodInfo = methodInfo;
            }

            protected override EarleValue Invoke(EarleStackFrame frame, EarleValue[] arguments)
            {
	            return Invoke(Name, _methodInfo, _target, frame, arguments);
            }
        }
    }
}
