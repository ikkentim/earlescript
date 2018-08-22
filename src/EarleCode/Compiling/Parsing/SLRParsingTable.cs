using System;
using System.Collections.Generic;
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
		private readonly List<Dictionary<ProductionRuleElement, SLRAction>> _table = new List<Dictionary<ProductionRuleElement, SLRAction>>();

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

			var cannonicalCollection = new LRCannonicalCollection(grammar);
			var sets = cannonicalCollection.Sets.ToArray();

			var first = new FirstSet(grammar);
			var follow = new FollowSet(grammar, first);

			for (var i = 0; i < sets.Length; i++)
			{
				var row = new Dictionary<ProductionRuleElement, SLRAction>();
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
						
						// TODO: NOT if followValue = S'
						foreach (var followValue in followSet)
						{
							var key = new ProductionRuleElement(followValue.Type, followValue.Value);

							if (row.TryGetValue(key, out var existing) && existing.Reduce != value.Rule)
								throw new GrammarException($"Ambiguous grammar near table[{_table.Count},{key}] = REDUCE {value.Rule}/{existing}");

							row[key] = new SLRAction(value.Rule);
						}
					}
					else
					{
						var element = value.Rule.Elements[value.Position];
						var goToValue = goTo[element];

						if (goToValue != null)
						{
							var index = Array.IndexOf(sets, goToValue);

							var type = element.Type != ProductionRuleElementType.NonTerminal
								? SLRActionType.Shift
								: SLRActionType.GoTo;
							
							if (row.TryGetValue(element, out var existing) && (existing.Type != type || 
							    existing.Value != index))
								throw new GrammarException($"Ambiguous grammar near table[{_table.Count},{element}] = {type} {index}/{existing}");

							row[element] = new SLRAction(index, type);
						}
					}
				}

				foreach (var key in goTo.Keys)
				{
					if (key.Type != ProductionRuleElementType.NonTerminal)
						continue;

					var index = Array.IndexOf(sets, goTo[key]);
					
					if (row.TryGetValue(key, out var existing) && (existing.Type != SLRActionType.GoTo || 
					    existing.Value != index))
						throw new GrammarException($"Ambiguous grammar near table[{_table.Count},{key}] = GOTO {index}/{existing}");

					row[key] = new SLRAction(index, SLRActionType.GoTo);
				}

				_table.Add(row);
			}
		}

		public SLRAction this[int state, ProductionRuleElement element]
		{
			get
			{
				if (state < 0 || state >= _table.Count) return SLRAction.Error;
				return _table[state].TryGetValue(element, out var value) ? value : SLRAction.Error;
			}
		}

		public int InitialState { get; } = -1;
	}
}