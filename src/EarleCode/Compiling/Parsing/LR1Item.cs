using System;
using System.Linq;
using EarleCode.Compiling.Parsing.Grammars;
using EarleCode.Compiling.Parsing.Grammars.Productions;

namespace EarleCode.Compiling.Parsing
{
    /// <summary>
    /// Represents a production rule with a cursor indicating how far the rule has been parsed and a single lookahead symbol.
    /// </summary>
    public struct LR1Item : IEquatable<LR1Item>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LR1Item"/> struct.
        /// </summary>
        /// <param name="rule">The production rule.</param>
        /// <param name="position">The position of the cursor indicating how far the rule has been parsed.</param>
        /// <param name="lookahead">The lookahead symbol.</param>
        public LR1Item(ProductionRule rule, int position, Terminal lookahead)
        {
            Rule = rule;
            Position = position;
            Lookahead = lookahead;
        }

        /// <summary>
        ///  Gets the production rule rule of the LR item.
        /// </summary>
        public ProductionRule Rule { get; }

        /// <summary>
        /// Gets the position of the cursor indicating how far the rule has been parsed.
        /// </summary>
        public int Position { get; }

        /// <summary>
        /// Gets the lookahead symbol.
        /// </summary>
        public Terminal Lookahead { get; }
        
        #region Overrides of Object

        /// <summary>
        ///     Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        ///     A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
			// •
			var pre = string.Join(" ", Rule.Elements.Take(Position).Select(n => n.ToString()));
	        var post = string.Join(" ", Rule.Elements.Skip(Position).Select(n => n.ToString()));
			return $"[{Rule.Name} -> {pre} . {post}, {Lookahead}]";
        }

        #endregion

        public bool Equals(LR1Item other)
        {
	        return Position == other.Position && Lookahead.Equals(other.Lookahead) && ReferenceEquals(Rule, other.Rule);
        }

        public override bool Equals(object obj)
        {
	        return obj is LR1Item other && Equals(other);
        }

        public override int GetHashCode()
        {
	        unchecked
	        {
		        var hashCode = (Rule != null ? Rule.GetHashCode() : 0);
		        hashCode = (hashCode * 397) ^ Position;
		        hashCode = (hashCode * 397) ^ Lookahead.GetHashCode();
		        return hashCode;
	        }
        }
    }
}