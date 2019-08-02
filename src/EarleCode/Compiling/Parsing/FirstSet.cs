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
    public class FirstSet : RuleTerminalSet
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
                    var noEpsilon = false;
                    if (rule.Elements.Length == 0)
                    {
                        changes |= Add(rule.Name, Terminal.Epsilon);
                    }
                    
                    foreach (var element in rule.Elements)
                    {
                        switch (element.Type)
                        {
                            case ProductionRuleElementType.Terminal:
                                changes |= Add(rule.Name, element.Terminal);
                                noEpsilon = true;
                                break;  
                            
                            case ProductionRuleElementType.NonTerminal:
                                var first = Get(element.Value);
                                changes |= Add(rule.Name, first);

                                // Only proceed to next element if no epsilon was found
                                if (first == null || !first.Any(x => x.Type == TerminalType.Epsilon))
                                    noEpsilon = true;
                                break;
                        }

                        if (noEpsilon)
                            break;
                    }
                }
            } while (changes);
        }
    }
}