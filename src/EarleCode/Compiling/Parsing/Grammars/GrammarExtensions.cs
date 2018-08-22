using System.Linq;
using EarleCode.Compiling.Parsing.Grammars.Productions;

namespace EarleCode.Compiling.Parsing.Grammars
{
    public static class GrammarExtensions
    {
        /// <summary>
        ///     Returns a value indicating whether the speified element can be empty.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns>A value indicating whether the speified element can be empty.</returns>
        public static bool ElementCanBeEmpty(this IGrammar grammar, ProductionRuleElement element)
        {
            return element.Type == ProductionRuleElementType.TerminalEmpty ||
                   element.Type == ProductionRuleElementType.NonTerminal && SymbolCanBeEmpty(grammar, element.Value);
        }

        /// <summary>
        ///     Returns a value indicating whether the specified symbol the can be empty.
        /// </summary>
        /// <param name="symbol">The symbol.</param>
        /// <returns>A value indicating whether the specified symbols the can be empty.</returns>
        public static bool SymbolCanBeEmpty(this IGrammar grammar, string symbol)
        {
            return grammar.Get(symbol).Any(ProductionRuleExtensions.CanBeEmpty);
        }
    }
}