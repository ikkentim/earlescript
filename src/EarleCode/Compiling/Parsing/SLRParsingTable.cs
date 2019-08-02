using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using EarleCode.Compiling.Parsing.Grammars;
using EarleCode.Compiling.Parsing.Grammars.Productions;

namespace EarleCode.Compiling.Parsing
{
    /// <summary>
    /// Represents an SLR(1) parsing table.
    /// </summary>
    public class SLRParsingTable
    {
        private readonly List<Dictionary<Terminal, SLRAction>> _actions = new List<Dictionary<Terminal, SLRAction>>();
        private readonly List<Dictionary<string, int>> _goTos = new List<Dictionary<string, int>>();

        /// <summary>
        /// Initializes a new instance of the <see cref="SLRParsingTable"/> class.
        /// </summary>
        /// <param name="grammar">The grammar to base the SLR parsing table on.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="grammar"/> is null.</exception>
        /// <exception cref="GrammarException">Thrown if the grammar is invalid.</exception>
        public SLRParsingTable(IGrammar grammar)
        {
            if (grammar == null) throw new ArgumentNullException(nameof(grammar));

            var startRules = grammar.Get(grammar.Default).ToArray();

            if (startRules.Length != 1)
                throw new GrammarException("Invalid default grammar rule. There must be exactly 1 start rule.");

            var startRule = startRules[0];

            Default = startRule;

            var cannonicalCollection = new LRCanonicalCollection(grammar);
            var sets = cannonicalCollection.Sets.ToArray();

            var first = new FirstSet(grammar);
            var follow = new FollowSet(grammar, first);

            for (var i = 0; i < sets.Length; i++)
            {
                var actionRow = new Dictionary<Terminal, SLRAction>();
                var goToRow = new Dictionary<string, int>();
                var set = sets[i];

                if (set.Any(s => s.Rule == startRule && s.Position == 0))
                    InitialState = i;

                var goTo = cannonicalCollection[set];

                foreach (var value in set)
                {
                    // If dot at end
                    if (value.Position >= value.Rule.Elements.Length)
                    {
                        var followSet = follow[value.Rule.Name];
						
                        if (followSet == null)
                            continue;
						
                        foreach (var followValue in followSet)
                        {
                            if (actionRow.TryGetValue(followValue, out var existing) && existing.Reduce != value.Rule)
                                throw new GrammarException($"Ambiguous grammar near table[{_actions.Count},{followValue}] = REDUCE {value.Rule}/{existing}");

                            actionRow[followValue] = new SLRAction(value.Rule);
                        }
                    }
                    else
                    {
                        var element = value.Rule.Elements[value.Position];
                        var goToValue = goTo[element];

                        if (goToValue == null) 
                            continue;
                        
                        var index = Array.IndexOf(sets, goToValue);

                        switch (element.Type)
                        {
                            case ProductionRuleElementType.Terminal when actionRow.TryGetValue(element.Terminal, out var existing) &&
                                                                         (existing.Type != SLRActionType.Shift || existing.Value != index):
                                throw new GrammarException($"Ambiguous grammar near actions[{_actions.Count},{element}] = SHIFT {index}/{existing}");
                            case ProductionRuleElementType.Terminal:
                                actionRow[element.Terminal] = new SLRAction(index, SLRActionType.Shift);
                                break;
                            case ProductionRuleElementType.NonTerminal when goToRow.TryGetValue(element.Value, out var existing) && existing != index:
                                throw new GrammarException($"Ambiguous grammar near table[{_goTos.Count},{element}] = GOTO {index}/{existing}");
                            case ProductionRuleElementType.NonTerminal:
                                goToRow[element.Value] = index;
                                break;
                            default:
                                throw new ParserException("Unknown value in parse table");
                        }
                    }
                }

                foreach (var key in goTo.Keys)
                {
                    if (key.Type != ProductionRuleElementType.NonTerminal)
                        continue;

                    var index = Array.IndexOf(sets, goTo[key]);
					
                    if (goToRow.TryGetValue(key.Value, out var existing) && existing != index)
                        throw new GrammarException($"Ambiguous grammar near table[{_actions.Count},{key}] = GOTO {index}/{existing}");

                    goToRow[key.Value] = index;
                }

                _actions.Add(actionRow);
                _goTos.Add(goToRow);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SLRParsingTable"/> class.
        /// </summary>
        /// <param name="initialState">The initial state.</param>
        /// <param name="table">The table data.</param>
        public SLRParsingTable(int initialState, List<Dictionary<Terminal, SLRAction>> actions, List<Dictionary<string, int>> goTos, ProductionRule @default)
        {
            InitialState = initialState;
            Default = @default ?? throw new ArgumentNullException(nameof(@default));
            _actions = actions ?? throw new ArgumentNullException(nameof(actions));
            _goTos = goTos ?? throw new ArgumentNullException(nameof(goTos));
        }

        public SLRAction this[int state, Terminal terminal]
        {
            get
            {
                if (state < 0 || state >= _actions.Count) throw new ArgumentOutOfRangeException(nameof(state), state, "Value should be between 0 and States");
                return _actions[state].TryGetValue(terminal, out var value) ? value : SLRAction.Error;
            }
        }

        /// <summary>
        /// Gets the GOTO value for the specified non-terminal. Returns -1 if no GOTO value is found.
        /// </summary>
        /// <param name="state"></param>
        /// <param name="nonTerminal"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public int this[int state, string nonTerminal]
        {
            get
            {
                if (state < 0 || state >= _actions.Count) throw new ArgumentOutOfRangeException(nameof(state), state, "Value should be between 0 and States");
                return _goTos[state].TryGetValue(nonTerminal, out var value) ? value : -1;
            }
        }

        public ProductionRule Default { get; }
        
        public int States => _actions.Count;

        public int InitialState { get; } = -1;

        public IReadOnlyDictionary<Terminal, SLRAction> GetRow(int state) =>
            new ReadOnlyDictionary<Terminal, SLRAction>(_actions[state]);
    }
}