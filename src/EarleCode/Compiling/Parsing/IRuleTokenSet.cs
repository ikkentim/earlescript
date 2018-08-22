using System.Collections.Generic;
using EarleCode.Compiling.Lexing;

namespace EarleCode.Compiling.Parsing
{
    /// <summary>
    /// Contains the methods of a rule to token collection dictionary.
    /// </summary>
    public interface IRuleTokenSet : IEnumerable<KeyValuePair<string, IEnumerable<Token>>>
    {
        /// <summary>
        /// Gets the tokens of the specified <paramref name="rule"/>.
        /// </summary>
        /// <param name="rule">The rule to get the tokens of.</param>
        IEnumerable<Token> this[string rule] { get; }

        /// <summary>
        /// Gets the tokens of the specified <paramref name="rule"/>.
        /// </summary>
        /// <param name="rule">The rule to get the tokens of.</param>
        /// <returns>The tokens of the rule.</returns>
        IEnumerable<Token> Get(string rule);
    }
}