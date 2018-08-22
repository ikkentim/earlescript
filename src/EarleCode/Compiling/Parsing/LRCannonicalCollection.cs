using System;
using System.Collections.Generic;
using System.Linq;
using EarleCode.Compiling.Parsing.Grammars;
using EarleCode.Compiling.Parsing.Grammars.Productions;

namespace EarleCode.Compiling.Parsing
{
	public class LRCannonicalCollection
	{
		private readonly Dictionary<LRClosure, LRGoToRow> _sets;

		/// <summary>
		/// Initializes a new instance of the <see cref="LRCannonicalCollection"/> instance.
		/// </summary>
		/// <param name="grammar">The grammar to compute the cannonical set with.</param>
		/// <exception cref="ArgumentNullException">Thrown when <see cref="ArgumentNullException"/> is null.</exception>
		/// <exception cref="GrammarException">Thrown when the grammar is invalid.</exception>
		public LRCannonicalCollection(IGrammar grammar)
		{
			if (grammar == null) throw new ArgumentNullException(nameof(grammar));

			var startRules = grammar.Get(grammar.Default).ToArray();

			if (startRules.Length != 1)
				throw new GrammarException("Invalid default grammar rule. There must be exactly 1 start rule.");

			var startRule = startRules[0];

			var initialItem = new LRItem(startRule);
			
			_sets = ComputeSets(grammar, initialItem);
		}

		/// <summary>
		/// Gets the sets of the collection.
		/// </summary>
		public IEnumerable<LRClosure> Sets => _sets.Keys;

		/// <summary>
		/// Gets the row for the specified set.
		/// </summary>
		/// <param name="set">The set.</param>
		public LRGoToRow this[LRClosure set] => GetGoTo(set);

		/// <summary>
		/// Gets the row for the specified set.
		/// </summary>
		/// <param name="set">The set.</param>
		/// <returns>The row.</returns>
		public LRGoToRow GetGoTo(LRClosure set)
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
		private Dictionary<LRClosure, LRGoToRow> ComputeSets(IGrammar grammar, LRItem initialItem)
		{
			var initial = ComputeClosure(grammar, initialItem);
			var closures = new Dictionary<LRClosure, LRGoToRow>
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
							closuresCopy.Add(new KeyValuePair<LRClosure, LRGoToRow>(n, goTo));
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
		private LRGoToRow ComputeGoTo(IGrammar grammar, LRClosure items)
		{
			var row = new LRGoToRow();

			foreach (var item in items)
			{
				if(item.Position >= item.Rule.Elements.Length) continue;

				var newItem = new LRItem(item.Rule, item.Position + 1);
				//row.Add(item.Rule.Elements[item.Position], newItem); TODO: This should be removable because it's also in the closure below.
				row.Add(item.Rule.Elements[item.Position], ComputeClosure(grammar, newItem));
			}

			return row;
		}

		/// <summary>
		/// Compure the closure of the specified item.
		/// </summary>
		/// <param name="grammar">The grammar to use.</param>
		/// <param name="closureItem">The item to compute the closure of.</param>
		/// <returns>The computed closure.</returns>
		private LRClosure ComputeClosure(IGrammar grammar, LRItem closureItem)
		{
			var closure = new LRClosure { closureItem };
			bool changes;
			do
			{
				changes = false;

				// Collection is modified during enumeration; cannot use foreach.
				// ReSharper disable once ForCanBeConvertedToForeach
				// ReSharper disable once LoopCanBeConvertedToQuery
				for (var i = 0; i < closure.Items.Count; i++)
				{
					var item = closure.Items[i];

					if (item.Position >= item.Rule.Elements.Length)
						continue;

					var element = item.Rule.Elements[item.Position];

					if (element.Type != ProductionRuleElementType.NonTerminal)
						continue;

					var rules = grammar.Get(element.Value);

					changes = rules.Aggregate(changes, (current, rule) => current | closure.Add(new LRItem(rule)));
				}
			} while (changes);

			return closure;
		}
		
	}
}