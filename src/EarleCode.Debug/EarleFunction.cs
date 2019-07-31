using System;
using System.Collections.Generic;
using EarleCode.Compiling.Earle.AST;

namespace EarleCode.Debug
{
	public class EarleFunction : IEarleFunction
	{
		public FunctionDeclaration Node { get; }

		public EarleFile File { get; }

		public IReadOnlyList<string> Parameters => Node.Parameters;

		public EarleFunction(EarleFile file, FunctionDeclaration node)
		{
			File = file ?? throw new ArgumentNullException(nameof(file));
			Node = node ?? throw new ArgumentNullException(nameof(node));
		}

		public int ParameterCount => Parameters?.Count ?? 0;

		public string Name => Node.Name;

		public IFrameExecutor GetFrameExecutor(EarleValue target, EarleValue[] args)
		{
			var frame = new InterpreterFrameExecutor
			{
				Function = this, 
				Interpreter = File.Interpreter,
				Target = target
			};
			
			var scope = new Scope(File.Interpreter);

			if(Parameters != null && args != null)
				for (var i = 0; i < Parameters.Count; i++)
					scope.SetInCurrent(Parameters[i], args.Length <= i ? EarleValue.Null : args[i]);

			frame.Scopes.Push(scope);

			return frame;
		}
	}
}