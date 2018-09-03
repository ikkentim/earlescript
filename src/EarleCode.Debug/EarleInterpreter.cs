using System;
using System.Collections.Generic;
using EarleCode.Compiling;

namespace EarleCode.Debug
{
	public class EarleInterpreter
	{
		private readonly EarleFileLoader _loader;
		private readonly Dictionary<string, EarleFile> _files = new Dictionary<string, EarleFile>();

		public EarleInterpreter(EarleFileLoader loader)
		{
			_loader = loader ?? throw new ArgumentNullException(nameof(loader));
			Compiler = new EarleCompiler();
		}

		public EarleCompiler Compiler { get; }

		public EarleFile this[string path] => GetFile(path);
		
		public EarleFile LoadFile(string path)
		{
			if (path == null) throw new ArgumentNullException(nameof(path));

			var input = _loader(path);

			var file = Compiler.Compile(input);

			return new EarleFile(file);
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
		
		public EarleFunction GetFunction(string name)
		{
			// TODO: IEarleFunction and not a switch-case
			if (name == "print")
			{
				return new Print(null, null);
			}

			return null;
		}

		public bool Invoke(EarleFunction function, out EarleValue result, params EarleValue[] args)
		{
			var frm = new InterpreterFrameExecutor
			{
				Function = function, 
				Interpreter = this,
				Target = EarleValue.Null
			};

			var scope = new Scope();

			if(function.Parameters != null && args != null)
				for (var i = 0; i < function.Parameters.Count; i++)
				{
					scope.Variables[function.Parameters[i]] = args.Length <= i ? EarleValue.Null : args[i];
				}
			frm.Scopes.Push(scope);

			var res = frm.Run();
			if (res != null)
			{
				result = res.Value;
				return true;
			}

			result = EarleValue.Null;
            
			return false;
		}
	}
}