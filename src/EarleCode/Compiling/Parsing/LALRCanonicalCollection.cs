using System;
using System.Collections.Generic;
using System.Linq;
using EarleCode.Compiling.Parsing.Grammars;
using EarleCode.Compiling.Parsing.Grammars.Productions;

namespace EarleCode.Compiling.Parsing
{
	public class LALRCanonicalCollection
	{
		private readonly Dictionary<Closure<LR1Item>, LRGoToRow<LR1Item>> _sets;

		public LALRCanonicalCollection(IGrammar grammar)
		{
			if (grammar == null) throw new ArgumentNullException(nameof(grammar));

			var startRules = grammar.Get(grammar.Default).ToArray();

			if (startRules.Length != 1)
				throw new GrammarException("Invalid default grammar rule. There must be exactly 1 start rule.");

			var startRule = startRules[0];

			var initialItem = new LR1Item(startRule, 0, Terminal.EndOfFile);

			_sets = ComputeSets(grammar, initialItem);
		}

		/// <summary>
		/// Gets the sets of the collection.
		/// </summary>
		public IEnumerable<Closure<LR1Item>> Sets => _sets.Keys;

		/// <summary>
		/// Gets the row for the specified set.
		/// </summary>
		/// <param name="set">The set.</param>
		public LRGoToRow<LR1Item> this[Closure<LR1Item> set] => GetGoTo(set);

		/// <summary>
		/// Gets the row for the specified set.
		/// </summary>
		/// <param name="set">The set.</param>
		/// <returns>The row.</returns>
		private LRGoToRow<LR1Item> GetGoTo(Closure<LR1Item> set)
		{
			_sets.TryGetValue(set, out var value);
			return value;
		}

		private Dictionary<Closure<LR1Item>, LRGoToRow<LR1Item>> ComputeSets(IGrammar grammar, LR1Item initialItem)
		{
			var first = new FirstSet(grammar);

			var initial = ComputeClosure(grammar, first, initialItem);

			var closures = new Dictionary<Closure<LR1Item>, LRGoToRow<LR1Item>>
			{
				[initial] = ComputeGoTo(grammar, first, initial)
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
							var goTo = ComputeGoTo(grammar, first, n);
							closuresCopy.Add(new KeyValuePair<Closure<LR1Item>, LRGoToRow<LR1Item>>(n, goTo));
							closures.Add(n, goTo);
							changes = true;
						}
					}
				}
			} while (changes);

			return closures;
		}

		private readonly Queue<LR1Item> _open = new Queue<LR1Item>();

		private Closure<LR1Item> ComputeClosure(IGrammar grammar, FirstSet first, LR1Item closureItem)
		{
			if (_cache.TryGetValue(closureItem, out var cache)) return cache;

			var closure = new Closure<LR1Item>();
			_open.Enqueue(closureItem);

			while (_open.Count > 0)
			{
				var item = _open.Dequeue();

				closure.Add(item);

				if (item.Position >= item.Rule.Elements.Length || item.Rule.Elements[item.Position].Type !=
				    ProductionRuleElementType.NonTerminal)
					continue;

				var nonTerminal = item.Rule.Elements[item.Position];
				var nextSymbol = item.Rule.Elements.Length > item.Position + 1
					? item.Rule.Elements[item.Position + 1]
					: null;
				var rules = grammar.Get(nonTerminal.Value);

				var firstSymbol = nextSymbol == null
					? new[] { item.Lookahead }
					: nextSymbol.Type == ProductionRuleElementType.Terminal
						? new[] { nextSymbol.Terminal }
						: first.Get(nextSymbol.Value);

				foreach (var rule in rules)
				{
					foreach (var terminal in firstSymbol)
					{
						var newItem = new LR1Item(rule, 0, terminal);
						if (!closure.Items.Contains(newItem) && !_open.Contains(newItem))
						{
							_open.Enqueue(newItem);
						}
					}
				}
			}

			_cache[closureItem] = closure;
			return closure;
		}

		private Dictionary<LR1Item, Closure<LR1Item>> _cache = new Dictionary<LR1Item, Closure<LR1Item>>();
		private Closure<LR1Item> ComputeClosureV0(IGrammar grammar, FirstSet first, LR1Item closureItem)
		{
			if (_cache.TryGetValue(closureItem, out var cache)) return cache;

			var closure = new Closure<LR1Item> { closureItem };

			bool changes;
			do
			{
				changes = false;

				foreach (var item in closure.Items.ToArray())
				{
					if (item.Position >= item.Rule.Elements.Length || item.Rule.Elements[item.Position].Type !=
					    ProductionRuleElementType.NonTerminal)
						continue;

					var nonTerminal = item.Rule.Elements[item.Position];
					var nextSymbol = item.Rule.Elements.Length > item.Position + 1
						? item.Rule.Elements[item.Position + 1]
						: null;
					var rules = grammar.Get(nonTerminal.Value);

					var f = nextSymbol == null
						? new[] { item.Lookahead }
						: nextSymbol.Type == ProductionRuleElementType.Terminal
							? new[] { nextSymbol.Terminal }
							: first.Get(nextSymbol.Value);

					foreach (var rule in rules)
					{
						foreach (var terminal in f)
						{
							changes |= closure.Add(new LR1Item(rule, 0, terminal));
						}
					}
				}
			} while (changes);

			_cache[closureItem] = closure;
			return closure;
		}

		private LRGoToRow<LR1Item> ComputeGoTo(IGrammar grammar, FirstSet first, Closure<LR1Item> items)
		{
			var row = new LRGoToRow<LR1Item>();

			foreach (var item in items)
			{
				if (item.Position >= item.Rule.Elements.Length) continue;

				var newItem = new LR1Item(item.Rule, item.Position + 1, item.Lookahead);
				row.Add(item.Rule.Elements[item.Position], ComputeClosure(grammar, first, newItem));
			}

			return row;
		}
	}
}