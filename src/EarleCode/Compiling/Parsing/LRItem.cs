using System;
using System.Linq;
using EarleCode.Compiling.Parsing.Grammars.Productions;

namespace EarleCode.Compiling.Parsing
{
	/// <summary>
	/// Represents a production rule with a cursor indicating how far the rule has been parsed.
	/// </summary>
	public struct LRItem
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="LRItem"/> struct.
		/// </summary>
		/// <param name="rule">The production rule.</param>
		/// <param name="position">The position of the cursor indicating how far the rule has been parsed.</param>
		public LRItem(ProductionRule rule, int position)
		{
			Rule = rule ?? throw new ArgumentNullException(nameof(rule));
			Position = position;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="LRItem"/> struct.
		/// </summary>
		/// <param name="rule">The production rule.</param>
		public LRItem(ProductionRule rule) : this(rule, 0)
		{
		}

		/// <summary>
		///  The production rule rule of the LR item.
		/// </summary>
		public ProductionRule Rule { get; }

		/// <summary>
		/// The position of the cursor indicating how far the rule has been parsed.
		/// </summary>
		public int Position { get; }

		#region Overrides of Object

		/// <summary>
		///     Returns a <see cref="System.String" /> that represents this instance.
		/// </summary>
		/// <returns>
		///     A <see cref="System.String" /> that represents this instance.
		/// </returns>
		public override string ToString()
		{

			return Rule.Name + " -> " +
			       string.Join(" ", Rule.Elements.Take(Position).Select(n => n.ToString())) +
			       " \u2022 " +
			       string.Join(" ", Rule.Elements.Skip(Position).Select(n => n.ToString()));
		}

		#endregion
	}
}