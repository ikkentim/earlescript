using System;
using System.Collections.Generic;
using EarleCode.Compiling.Earle.AST;

namespace EarleCode.Debug
{
	public class EarleFunction
	{
		public FunctionDeclaration Node { get; }

		public EarleFile File { get; }

		public virtual IReadOnlyList<string> Parameters => Node.Parameters;

		public EarleFunction(EarleFile file, FunctionDeclaration node)
		{
			File = file;// ?? throw new ArgumentNullException(nameof(file));
			Node = node;// ?? throw new ArgumentNullException(nameof(node));
		}
	}
}