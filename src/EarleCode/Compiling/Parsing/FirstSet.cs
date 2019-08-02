using System;
using System.Linq;
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
                    var doBreak = false;
                    foreach (var element in rule.Elements)
                    {
                        switch (element.Type)
                        {
                            case ProductionRuleElementType.Terminal:
                                changes |= Add(rule.Name, element.Token);
                                doBreak = true;
                                break;
                            case ProductionRuleElementType.TerminalEmpty:
                                changes |= Add(rule.Name, Token.Empty);
                                doBreak = true;
                                break;
                                
                            case ProductionRuleElementType.NonTerminal:
                                var s = Get(element.Value);
                                changes |= Add(rule.Name, s);

                                
                                if (s == null || !s.Any(x => x.IsEmpty))//grammar.SymbolCanBeEmpty(element.Value))
                                    doBreak = true;
                                break;
                        }

                        if (doBreak)
                            break;
                    }

//                    if (!doBreak)
//                        changes |= Add(rule.Name, Token.Empty);
                }
            } while (changes);
        }
    }
}