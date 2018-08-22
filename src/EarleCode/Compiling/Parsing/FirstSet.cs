using System;
using EarleCode.Compiling.Lexing;
using EarleCode.Compiling.Parsing.Grammars;
using EarleCode.Compiling.Parsing.Grammars.Productions;

namespace EarleCode.Compiling.Parsing
{
    /// <summary>
    /// Represents a FIRST set.
    /// </summary>
    public class FirstSet : RuleTokenSet
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FirstSet"/> class.
        /// </summary>
        /// <param name="grammar">The grammar to build the first set with.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="grammar"/> is null.</exception>
        public FirstSet(IGrammar grammar)
        {
            if (grammar == null) throw new ArgumentNullException(nameof(grammar));

            bool changes;
            do
            {
                changes = false;

                foreach (var rule in grammar.All)
                {
                    var any = false;
                    foreach (var element in rule.Elements)
                    {
                        switch (element.Type)
                        {
                            case ProductionRuleElementType.Terminal:
                                changes |= Add(rule.Name, element.Token);
                                any = true;
                                break;
                            case ProductionRuleElementType.NonTerminal:
                                changes |= Add(rule.Name, Get(element.Value));

                                if (!grammar.SymbolCanBeEmpty(element.Value))
                                    any = true;
                                break;
                        }

                        if (any)
                            break;
                    }

                    if (!any)
                        changes |= Add(rule.Name, Token.Empty);
                }
            } while (changes);
        }
    }
}