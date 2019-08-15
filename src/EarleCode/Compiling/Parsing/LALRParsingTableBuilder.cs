using System.Collections.Generic;
using System.Linq;
using EarleCode.Compiling.Parsing.Grammars;
using EarleCode.Compiling.Parsing.Grammars.Productions;

namespace EarleCode.Compiling.Parsing
{
	public class LALRParsingTableBuilder
	{
		public ShiftReduceParsingTable Build(IGrammar g)
		{
			LALRCanonicalCollection c = new LALRCanonicalCollection(g);

			var x = new CanonicalParsingTable(g, c);

			var sets = c.Sets.ToArray();

			var merges = new List<(int, int)>();
			for (var i = 0; i < sets.Length; i++)
			{
				for (var j = i + 1; j < sets.Length; j++)
				{
					var ii = sets[i].Items;
					var jj = sets[j].Items;

					if (ii.Count <= jj.Count)
					{
						if (ii.All(lhs => jj.Any(rhs => lhs.Position == rhs.Position && lhs.Rule == rhs.Rule)))
						{
							merges.Add((i, j));
						}
					}
					else
					{
						if (jj.All(lhs => ii.Any(rhs => lhs.Position == rhs.Position && lhs.Rule == rhs.Rule)))
						{
							merges.Add((i, j));
						}
					}
				}
			}

			if (merges.Count == 0)
				return x;

			var newTable = new ShiftReduceParsingTable
			{
				Default = x.Default,
				InitialState = x.InitialState,
				States = x.States - merges.Count
			};

			for (var oldState = 0; oldState < x.States; oldState++)
			{
				if (merges.Any(z => z.Item2 == oldState))
					continue;

				var newState = oldState - merges.Count(z => z.Item2 < oldState);

				// copy
				foreach (var t in g.Terminals)
				{
					if (x.TryGetAction(oldState, t, out var a))
					{
						if (a.Type == SLRActionType.Shift)
						{
							var value = a.Value;
							foreach (var m in merges)
							{
								if (m.Item2 == value)
								{
									value = m.Item1;
									break;
								}
							}
							value -= merges.Count(z => z.Item2 < value);

							a = new SLRAction(value, SLRActionType.Shift);
						}
						newTable[newState, t] = a;
					}
				}
				foreach (var n in g.NonTerminals)
				{
					if (x.TryGetGoTo(oldState, n, out var gt))
					{
						foreach (var m in merges)
						{
							if (m.Item2 == gt)
							{
								gt = m.Item1;
								break;
							}
						}
						gt -= merges.Count(z => z.Item2 < gt);
						newTable[newState, n] = gt;
					}
				}

				// merge
				foreach (var merge in merges.Where(m => m.Item1 == oldState))
				{
					foreach (var t in g.Terminals)
					{
						if (x.TryGetAction(merge.Item2, t, out var a))
						{
							if (a.Type == SLRActionType.Shift)
							{
								var value = a.Value;
								foreach (var m in merges)
								{
									if (m.Item2 == value)
									{
										value = m.Item1;
										break;
									}
								}
								value -= merges.Count(z => z.Item2 < value);

								a = new SLRAction(value, SLRActionType.Shift);
							}
							newTable[newState, t] = a;
						}
					}
					foreach (var n in g.NonTerminals)
					{
						if (x.TryGetGoTo(merge.Item2, n, out var gt))
						{
							foreach (var m in merges)
							{
								if (m.Item2 == gt)
								{
									gt = m.Item1;
									break;
								}
							}
							gt -= merges.Count(z => z.Item2 < gt);
							newTable[newState, n] = gt;
						}
					}
				}
			}

			return newTable;
		}
	}
}