using System;
using System.Collections.Generic;
using EarleCode.Compiling.Earle.AST;

namespace EarleCode.Interpreter
{
	public class EarleFile
	{
		private readonly Dictionary<string, EarleFunction> _functions;
		
		public EarleFile(EarleInterpreter interpreter, ProgramFile node)
		{
			Interpreter = interpreter ?? throw new ArgumentNullException(nameof(interpreter));
			Node = node ?? throw new ArgumentNullException(nameof(node));

			if (node.FunctionDeclarations == null)
				return;

			_functions = new Dictionary<string, EarleFunction>();
			foreach (var function in node.FunctionDeclarations)
				_functions[function.Name] = new EarleFunction(this, function);
		}

		public EarleInterpreter Interpreter { get; }

		public ProgramFile Node { get; }

		public EarleFunction this[string name] => GetFunction(name);

		private EarleFunction GetFunction(string name)
		{
			if (_functions == null)
				return null;

			_functions.TryGetValue(name, out var result);
			return result;
		}
	}
}