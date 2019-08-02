using System;
using System.Linq;
using EarleCode.Compiling.Lexing;
using EarleCode.Compiling.Parsing.Grammars;
using EarleCode.Compiling.Parsing.Grammars.Productions;

namespace EarleCode.Compiling.Parsing
{
    /// <summary>
    /// Represents a FOLLOW set.
    /// </summary>
    public class FollowSet : RuleTokenSet
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FollowSet"/> class.
        /// </summary>
        /// <param name="grammar">The grammar to build the follow set with.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="grammar"/> is null.</exception>
	    public FollowSet(IGrammar grammar) : this(grammar, new FirstSet(grammar))
	    {
	    }

        /// <summary>
        /// Initializes a new instance of the <see cref="FollowSet"/> class.
        /// </summary>
        /// <param name="grammar">The grammar to build the follow set with.</param>
        /// <param name="firstSet">The first set to build the follow set with.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="grammar"/> or <paramref name="firstSet"/> is null.</exception>
	    public FollowSet(IGrammar grammar, IRuleTokenSet firstSet)
        {
	        if (grammar == null) throw new ArgumentNullException(nameof(grammar));
	        if (firstSet == null) throw new ArgumentNullException(nameof(firstSet));

	        bool changes;
            
            Add(grammar.Default, Token.EndOfFile);

            do
            {
                changes = false;

                foreach (var rule in grammar.All)
                    for (var i = 0; i < rule.Elements.Length; i++)
                    {
                        var element = rule.Elements[i];
                        // Find non-terminal elements in all rules.
                        if (element.Type != ProductionRuleElementType.NonTerminal)
                            continue;
                        
                        // If the non-terminal is the last element, add FOLLOW(rule) to FOLLOW(non-terminal)
                        if (i == rule.Elements.Length - 1)
                        {
                            changes |= Add(element.Value, Get(rule.Name));
                            continue;
                        }

                        var nextElement = rule.Elements[i + 1];

                        switch (nextElement.Type)
                        {
                            case ProductionRuleElementType.Terminal:
                                changes |= Add(element.Value, nextElement.Token);
                                break;
                            case ProductionRuleElementType.NonTerminal:
                                changes |= Add(element.Value, firstSet.Get(nextElement.Value).Where(t => !t.IsEmpty));
                                break;
                            case ProductionRuleElementType.TerminalEmpty:
                                throw new NotImplementedException();
                        }

                        var remainingElementsCanBeEmpty = rule.Elements.Skip(i + 1).All(grammar.ElementCanBeEmpty);
                        if (remainingElementsCanBeEmpty)
                            changes |= Add(element.Value, Get(rule.Name));
                    }
            } while (changes);
        }
    }
}