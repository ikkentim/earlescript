using System.Collections.Generic;
using EarleCode.Compiling.Earle.AST;

namespace EarleCode.Debug
{
	public class Print : EarleFunction
	{
		public Print(EarleFile file, FunctionDeclaration node) : base(file, node)
		{
		}

		#region Overrides of EarleFunction

		public override IReadOnlyList<string> Parameters { get; } = new []{"value"};

		#endregion
	}
}