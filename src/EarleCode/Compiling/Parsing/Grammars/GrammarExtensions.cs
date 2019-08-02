using System.Linq;
using EarleCode.Compiling.Parsing.Grammars.Productions;

namespace EarleCode.Compiling.Parsing.Grammars
{
    public static class GrammarExtensions
    {
        /// <summary>
        ///     Returns a value indicating whether the specified element can be empty.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns>A value indicating whether the specified element can be epsilon.</returns>
        public static bool ElementCanBeEpsilon(this IGrammar grammar, ProductionRuleElement element)
        {
            return element.Type == ProductionRuleElementType.NonTerminal && SymbolCanBeEpsilon(grammar, element.Value);
        }

        /// <summary>
        ///     Returns a value indicating whether the specified symbol the can be epsilon.
        /// </summary>
        /// <param name="symbol">The symbol.</param>
        /// <returns>A value indicating whether the specified symbols the can be epsilon.</returns>
        public static bool SymbolCanBeEpsilon(this IGrammar grammar, string symbol)
        {
            return grammar.Get(symbol).Any(r => r.IsEpsilon);
        }
    }
}