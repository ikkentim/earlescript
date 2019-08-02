using System.Collections.Generic;
using EarleCode.Compiling.Parsing.Grammars;

namespace EarleCode.Compiling.Parsing
{
    /// <summary>
    /// Contains the methods of a rule to terminal collection dictionary.
    /// </summary>
    public interface IRuleTerminalSet : IEnumerable<KeyValuePair<string, IEnumerable<Terminal>>>
    {
        /// <summary>
        /// Gets the terminals of the specified <paramref name="rule"/>.
        /// </summary>
        /// <param name="rule">The rule to get the terminals of.</param>
        IEnumerable<Terminal> this[string rule] { get; }

        /// <summary>
        /// Gets the terminals of the specified <paramref name="rule"/>.
        /// </summary>
        /// <param name="rule">The rule to get the terminals of.</param>
        /// <returns>The terminals of the rule.</returns>
        IEnumerable<Terminal> Get(string rule);
    }
}