using System;
using System.Collections;
using System.Collections.Generic;

namespace EarleCode.Debug
{
	public class EarleNativeFunctionCollection
	{
		private readonly Dictionary<string, IEarleFunction> _functions = new Dictionary<string, IEarleFunction>();

		public IEarleFunction this[string name] => Get(name);

		private IEarleFunction Get(string name)
		{
			_functions.TryGetValue(name, out var value);
			return value;
		}

		public void Register(IEarleFunction function)
		{
			//TODO: Throw on dupe names?
			_functions[function.Name] = function;
		}

		public void Register(string name, int parameterCount, Func<EarleValue[], EarleValue> func)
		{
			Register(new FuncFunction(name, parameterCount, func));
		}

		public void Register(string name, int parameterCount, Func<EarleValue[], IEnumerator> func)
		{
			Register(new CoroutineFunction(name, parameterCount, func));
		}

		private class CoroutineFunction : IEarleFunction
		{
			private readonly Func<EarleValue[], IEnumerator> _func;

			public CoroutineFunction(string name, int parameterCount, Func<EarleValue[], IEnumerator> func)
			{
				ParameterCount = parameterCount;
				Name = name ?? throw new ArgumentNullException(nameof(name));
				_func = func ?? throw new ArgumentNullException(nameof(func));
			}

			#region Implementation of IEarleFunction

			public int ParameterCount { get; }

			public string Name { get; }

			public IFrameExecutor GetFrameExecutor(EarleValue[] args)
			{
				return new Executor(this, args);
			}

			#endregion

			private class Executor : IFrameExecutor
			{
				private readonly CoroutineFunction _func;
				private readonly IEnumerator _enumerator;

				public Executor(CoroutineFunction func, EarleValue[] args)
				{
					_func = func;
					_enumerator = _func._func(args);
				}

				#region Implementation of IFrameExecutor

				public string Name => _func.Name;

				public EarleValue? Run(IFrameExecutor parent)
				{
					if (_enumerator.MoveNext())
					{
						return null;
					}

					return _enumerator.Current is EarleValue result ? result : EarleValue.Null;
				}

				#endregion
			}
		}

		private class FuncFunction : IEarleFunction
		{
			private readonly Func<EarleValue[], EarleValue> _func;

			public FuncFunction(string name, int parameterCount, Func<EarleValue[], EarleValue> func)
			{
				ParameterCount = parameterCount;
				Name = name ?? throw new ArgumentNullException(nameof(name));
				_func = func ?? throw new ArgumentNullException(nameof(func));
			}

			#region Implementation of IEarleFunction

			public int ParameterCount { get; }
			public string Name { get; }

			public IFrameExecutor GetFrameExecutor(EarleValue[] args)
			{
				return new Executor(this, args);
			}

			#endregion

			private class Executor : IFrameExecutor
			{
				private readonly FuncFunction _func;
				private readonly EarleValue[] _args;

				public Executor(FuncFunction func, EarleValue[] args)
				{
					_func = func;
					_args = args;
				}

				#region Implementation of IFrameExecutor

				public string Name => _func.Name;

				public EarleValue? Run(IFrameExecutor parent)
				{
					return _func._func(_args);
				}

				#endregion
			}
		}
	}
}