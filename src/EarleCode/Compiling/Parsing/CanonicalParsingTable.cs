using System;
using System.Linq;
using EarleCode.Compiling.Parsing.Grammars;
using EarleCode.Compiling.Parsing.Grammars.Productions;

namespace EarleCode.Compiling.Parsing
{
	public class CanonicalParsingTable : ShiftReduceParsingTable
	{
		public CanonicalParsingTable(IGrammar grammar, LALRCanonicalCollection canonicalCollection = null)
		{
			if (grammar == null) throw new ArgumentNullException(nameof(grammar));

			var startRules = grammar.Get(grammar.Default).ToArray();

			if (startRules.Length != 1)
				throw new GrammarException("Invalid default grammar rule. There must be exactly 1 start rule.");

			var startRule = startRules[0];

			Default = startRule;

			if (canonicalCollection == null)
				canonicalCollection = new LALRCanonicalCollection(grammar);
			var sets = canonicalCollection.Sets.ToArray();

			States = sets.Length;
			for (var state = 0; state < sets.Length; state++)
			{
				var set = sets[state];

				if (set.Any(s => s.Rule == startRule && s.Position == 0))
					InitialState = state;

				var goTo = canonicalCollection[set];

				foreach (var value in set)
				{
					// If dot at end
					if (value.Position >= value.Rule.Elements.Length)
					{
						var followValue = value.Lookahead;
						if (TryGetAction(state, followValue, out var existing) && existing.Reduce != value.Rule)
							throw new GrammarException(
								$"Ambiguous grammar near table[{state},{followValue}] = REDUCE {value.Rule}/{existing}");

						this[state, followValue] = new SLRAction(value.Rule);

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
							case ProductionRuleElementType.Terminal
								when TryGetAction(state, element.Terminal, out var existing) &&
								     (existing.Type != SLRActionType.Shift || existing.Value != index):
								throw new GrammarException(
									$"Ambiguous grammar near actions[{state},{element}] = SHIFT {index}/{existing}");
							case ProductionRuleElementType.Terminal:
								this[state, element.Terminal] = new SLRAction(index, SLRActionType.Shift);
								break;
							case ProductionRuleElementType.NonTerminal
								when TryGetGoTo(state, element.Value, out var existing) && existing != index:
								throw new GrammarException(
									$"Ambiguous grammar near table[{state},{element}] = GOTO {index}/{existing}");
							case ProductionRuleElementType.NonTerminal:
								this[state, element.Value] = index;
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

					if (TryGetGoTo(state, key.Value, out var existing) && existing != index)
						throw new GrammarException(
							$"Ambiguous grammar near table[{state},{key}] = GOTO {index}/{existing}");

					this[state, key.Value] = index;
				}
			}
		}
	}
}