using System;
using EarleCode.Compiling.Parsing.Grammars;

namespace EarleCode.Compiling.Parsing
{
	/// <summary>
	/// Represents an SLR(1) parser.
	/// </summary>
	public class SLRParser : ShiftReduceParser
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="SLRParser"/> class.
		/// </summary>
		/// <param name="grammar">The grammar to build the parse table with.</param>
		public SLRParser(IGrammar grammar) : base(
			new SLRParsingTable(grammar ?? throw new ArgumentNullException(nameof(grammar))))
		{
		}
	}
}