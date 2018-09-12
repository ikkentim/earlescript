using System;
using System.Collections.Generic;
using EarleCode.Compiling;

namespace EarleCode.Debug
{
	public class EarleInterpreter : IScope
	{
		private readonly EarleFileLoader _loader;
		private readonly Dictionary<string, EarleFile> _files = new Dictionary<string, EarleFile>();
		private readonly List<CallQueueEntry> _queue = new List<CallQueueEntry>(64);
		
		public EarleInterpreter(EarleFileLoader loader)
		{
			_loader = loader ?? throw new ArgumentNullException(nameof(loader));
			Compiler = new EarleCompiler();
		}

		public EarleCompiler Compiler { get; }

		public EarleFile this[string path] => GetFile(path);
		
		public EarleNativeFunctionCollection Natives { get; } = new EarleNativeFunctionCollection();

		EarleValue IScope.this[string name] => EarleValue.Null;

		public EarleFile LoadFile(string path)
		{
			if (path == null) throw new ArgumentNullException(nameof(path));

			if (!EarlePath.TryParse(path, out var value))
				throw new RuntimeException("Malformed script file path");

			var input = _loader(value);

			var file = Compiler.Compile(input);

			return new EarleFile(this, file);
		}

		private EarleFile GetFile(string path)
		{
			if (path == null) throw new ArgumentNullException(nameof(path));

			if (_files.TryGetValue(path, out var file))
				return file;

			file = LoadFile(path);
			_files[path] = file;

			return file;
		}

		public IEarleFunction GetFunction(string name)
		{
			return Natives[name];
		}

		public void Tick()
		{
			// TODO: Optimize; lists are bad for this
			for (var i = _queue.Count - 1; i >= 0; i--)
			{
				 var res = _queue[i].frame.Run(null);

				if (res != null)
				{
					_queue[i].handler?.Invoke(res.Value);
					_queue.RemoveAt(i);
				}
			}
		}

		public bool Invoke(IEarleFunction function, out EarleValue result, params EarleValue[] args)
		{
			var frame = function.GetFrameExecutor(args);
			var res = frame.Run(null);

			if (res != null)
			{
				result = res.Value;
				return true;
			}

			result = EarleValue.Null;
			_queue.Add(new CallQueueEntry
			{
				frame = frame
			});


			return false;
		}
		
		public bool Invoke(IEarleFunction function, Action<EarleValue> resultHandler, params EarleValue[] args)
		{
			var frame = function.GetFrameExecutor(args);
			var res = frame.Run(null);

			if (res != null)
			{
				resultHandler?.Invoke(res.Value);
				return true;
			}

			_queue.Add(new CallQueueEntry
			{
				handler = resultHandler, 
				frame = frame
			});
			
			return false;
		}


		bool IScope.SetIfPresent(string name, EarleValue value)
		{
			return false;
		}

		private struct CallQueueEntry
		{
			public Action<EarleValue> handler;
			public IFrameExecutor frame;
		}
	}
}