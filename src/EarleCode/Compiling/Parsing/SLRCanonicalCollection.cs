using System;
using System.Collections.Generic;
using System.Linq;
using EarleCode.Compiling.Parsing.Grammars;
using EarleCode.Compiling.Parsing.Grammars.Productions;

namespace EarleCode.Compiling.Parsing
{
    public class SLRCanonicalCollection
    {
        private readonly List<LR0Item> _itemBuffer = new List<LR0Item>();

        private readonly Dictionary<Closure<LR0Item>, LRGoToRow<LR0Item>> _sets;

        /// <summary>
        /// Initializes a new instance of the <see cref="SLRCanonicalCollection"/> instance.
        /// </summary>
        /// <param name="grammar">The grammar to compute the canonical set with.</param>
        /// <exception cref="ArgumentNullException">Thrown when <see cref="ArgumentNullException"/> is null.</exception>
        /// <exception cref="GrammarException">Thrown when the grammar is invalid.</exception>
        public SLRCanonicalCollection(IGrammar grammar)
        {
            if (grammar == null) throw new ArgumentNullException(nameof(grammar));

            var startRules = grammar.Get(grammar.Default).ToArray();

            if (startRules.Length != 1)
                throw new GrammarException("Invalid default grammar rule. There must be exactly 1 start rule.");

            var startRule = startRules[0];

            var initialItem = new LR0Item(startRule);
			
            _sets = ComputeSets(grammar, initialItem);
        }

        /// <summary>
        /// Gets the sets of the collection.
        /// </summary>
        public IEnumerable<Closure<LR0Item>> Sets => _sets.Keys;

        /// <summary>
        /// Gets the row for the specified set.
        /// </summary>
        /// <param name="set">The set.</param>
        public LRGoToRow<LR0Item> this[Closure<LR0Item> set] => GetGoTo(set);

        /// <summary>
        /// Gets the row for the specified set.
        /// </summary>
        /// <param name="set">The set.</param>
        /// <returns>The row.</returns>
        private LRGoToRow<LR0Item> GetGoTo(Closure<LR0Item> set)
        {
            _sets.TryGetValue(set, out var value);
            return value;
        }

        /// <summary>
        /// Computes the sets for the grammar with the specified initial LR item.
        /// </summary>
        /// <param name="grammar">The grammar to compute the sets with.</param>
        /// <param name="initialItem">The initial LR item to compute the sets with.</param>
        /// <returns>The computed sets.</returns>
        private Dictionary<Closure<LR0Item>, LRGoToRow<LR0Item>> ComputeSets(IGrammar grammar, LR0Item initialItem)
        {
            var initial = ComputeClosure(grammar, initialItem);
            var closures = new Dictionary<Closure<LR0Item>, LRGoToRow<LR0Item>>
            {
                [initial] = ComputeGoTo(grammar, initial)
            };

            bool changes;

            do
            {
                changes = false;
                var closuresCopy = closures.ToList();
                for (var i = 0; i < closuresCopy.Count; i++)
                {
                    var @goto = closuresCopy[i].Value;

                    foreach (var n in @goto.Values)
                    {
                        if (!closures.Any(closure => closure.Key.Equals(n)))
                        {
                            var goTo = ComputeGoTo(grammar, n);
                            closuresCopy.Add(new KeyValuePair<Closure<LR0Item>, LRGoToRow<LR0Item>>(n, goTo));
                            closures.Add(n, goTo);
                            changes = true;
                        }
                    }
                }
            } while (changes);

            return closures;
        }

        /// <summary>
        /// Computes the go-to rule for the specified closure.
        /// </summary>
        /// <param name="grammar">The grammar to use.</param>
        /// <param name="items">The closure to compute the go-to rule for.</param>
        /// <returns>The computed row.</returns>
        private LRGoToRow<LR0Item> ComputeGoTo(IGrammar grammar, Closure<LR0Item> items)
        {
            var row = new LRGoToRow<LR0Item>();

            foreach (var item in items)
            {
                if(item.Position >= item.Rule.Elements.Length) continue;

                var newItem = new LR0Item(item.Rule, item.Position + 1);
                row.Add(item.Rule.Elements[item.Position], ComputeClosure(grammar, newItem));
            }

            return row;
        }

        /// <summary>
        /// Computes the closure of the specified item.
        /// </summary>
        /// <param name="grammar">The grammar to use.</param>
        /// <param name="closureItem">The item to compute the closure of.</param>
        /// <returns>The computed closure.</returns>
        private Closure<LR0Item> ComputeClosure(IGrammar grammar, LR0Item closureItem)
        {
            var closure = new Closure<LR0Item> { closureItem };
            bool changes;
			
            if (_itemBuffer.Capacity < closure.Items.Count)
                _itemBuffer.Capacity = closure.Items.Count;

            foreach (var item in closure.Items)
                _itemBuffer.Add(item);

            do
            {
                changes = false;
				
                // Collection is modified during enumeration; cannot use foreach.
                for (var i = _itemBuffer.Count - 1; i >= 0; i--)
                {
                    var item = _itemBuffer[i];

                    if (item.Position >= item.Rule.Elements.Length)
                        continue;

                    var element = item.Rule.Elements[item.Position];

                    if (element.Type != ProductionRuleElementType.NonTerminal)
                        continue;

                    var rules = grammar.Get(element.Value);

                    foreach (var rule in rules)
                    {
                        var newItem = new LR0Item(rule);
                        if (closure.Add(newItem))
                        {
                            changes = true;
                            _itemBuffer.Add(newItem);
                        }
                    }
                }
            } while (changes);
			
            _itemBuffer.Clear();

            return closure;
        }
		
    }
}