using System;
using System.Collections.Generic;

namespace CodeProject.Syntax.LALR
{
	public class LalrPropagation
	{
		public int Lr0TargetState { get; set; }
		public int Lr0TargetItem { get; set; }
	};

	public class Parser
	{
		public HashSet<int>[] FirstSets { get; private set; }

		public List<Lr0Item> Lr0Items { get; }

		public List<Lr1Item> Lr1Items { get; }

		public List<HashSet<int>> Lr0States { get; }

		public List<HashSet<int>> Lr0Kernels { get; }

		public List<HashSet<int>> LalrStates { get; }

		public List<int[]> LrGoto { get; }

		public List<int[]> GotoPrecedence { get; }

		public List<Production> Productions { get; }

		public List<int> Terminals { get; }

		public List<int> NonTerminals { get; }

		public List<Dictionary<int, List<LalrPropagation>>> LalrPropagations { get; }

		public List<int> ProductionPrecedence { get; }

		public List<Derivation> ProductionDerivation { get; }

		public Grammar Grammar { get; }

		public ParseTable ParseTable { get; private set; }


		/// <summary>
		/// Adds a propagation to the propagation table
		/// </summary>
		private void AddPropagation(int nLr0SourceState, int nLr0SourceItem, int nLr0TargetState, int nLr0TargetItem)
		{
			while (LalrPropagations.Count <= nLr0SourceState)
			{
				LalrPropagations.Add(new Dictionary<int, List<LalrPropagation>>());
			}

			var propagationsForState = LalrPropagations[nLr0SourceState];
			if (!propagationsForState.TryGetValue(nLr0SourceItem, out var propagationList))
			{
				propagationList = new List<LalrPropagation>();
				propagationsForState[nLr0SourceItem] = propagationList;
			}

			propagationList.Add(new LalrPropagation {Lr0TargetState = nLr0TargetState, Lr0TargetItem = nLr0TargetItem});
		}

		/// <summary>
		/// Gets the ID for a particular LR0 Item
		/// </summary>
		private int GetLr0ItemId(Lr0Item item)
		{
			var nItemId = 0;
			foreach (var oItem in Lr0Items)
			{
				if (oItem.Equals(item))
				{
					return nItemId;
				}

				nItemId++;
			}

			Lr0Items.Add(item);
			return nItemId;
		}


		/// <summary>
		/// Gets the ID for a particular LR1 Item
		/// </summary>
		private int GetLr1ItemId(Lr1Item item)
		{
			var nItemId = 0;
			foreach (var oItem in Lr1Items)
			{
				if (oItem.Equals(item))
				{
					return nItemId;
				}

				nItemId++;
			}

			Lr1Items.Add(item);
			return nItemId;
		}


		/// <summary>
		/// Gets the ID for a particular LR0 State
		/// </summary>
		private int GetLr0StateId(HashSet<int> state, ref bool bAdded)
		{
			var nStateId = 0;
			foreach (var oState in Lr0States)
			{
				if (oState.SetEquals(state))
				{
					return nStateId;
				}

				nStateId++;
			}

			Lr0States.Add(state);
			bAdded = true;
			return nStateId;
		}

		/// <summary>
		/// takes a set of LR0 Items and Produces all of the LR0 Items that are reachable by substitution
		/// </summary>
		private HashSet<int> Lr0Closure(HashSet<int> i)
		{
			var closed = new HashSet<int>();
			var open = new List<int>();

			foreach (var itemCopy in i)
			{
				open.Add(itemCopy);
			}

			while (open.Count > 0)
			{
				var nItem = open[0];
				open.RemoveAt(0);
				var item = Lr0Items[nItem];
				closed.Add(nItem);

				var nProduction = 0;
				foreach (var production in Productions)
				{
					if ((item.Position < Productions[item.Production].Right.Length) &&
					    (production.Left == Productions[item.Production].Right[item.Position]))
					{
						var newItem = new Lr0Item {Production = nProduction, Position = 0};
						var nNewItemId = GetLr0ItemId(newItem);
						if (!open.Contains(nNewItemId) && !closed.Contains(nNewItemId))
						{
							open.Add(nNewItemId);
						}
					}

					nProduction++;
				}
			}

			return closed;
		}

		/// <summary>
		/// takes a set of LR1 Items (LR0 items with look-ahead) and produces all of those LR1 items reachable by substitution
		/// </summary>
		private HashSet<int> Lr1Closure(HashSet<int> i)
		{
			var closed = new HashSet<int>();
			var open = new List<int>();

			foreach (var itemCopy in i)
			{
				open.Add(itemCopy);
			}

			while (open.Count > 0)
			{
				var nLr1Item = open[0];
				open.RemoveAt(0);
				var lr1Item = Lr1Items[nLr1Item];
				var lr0Item = Lr0Items[lr1Item.Lr0ItemId];
				closed.Add(nLr1Item);

				if (lr0Item.Position < Productions[lr0Item.Production].Right.Length)
				{
					var nToken = Productions[lr0Item.Production].Right[lr0Item.Position];
					if (NonTerminals.Contains(nToken))
					{
						var argFirst = new List<int>();
						for (var nIdx = lr0Item.Position + 1;
							nIdx < Productions[lr0Item.Production].Right.Length;
							nIdx++)
						{
							argFirst.Add(Productions[lr0Item.Production].Right[nIdx]);
						}

						var first = First(argFirst, lr1Item.LookAhead);
						var nProduction = 0;
						foreach (var production in Productions)
						{
							if (production.Left == nToken)
							{
								foreach (var nTokenFirst in first)
								{
									var newLr0Item = new Lr0Item {Production = nProduction, Position = 0};
									var nNewLr0ItemId = GetLr0ItemId(newLr0Item);
									var newLr1Item = new Lr1Item {Lr0ItemId = nNewLr0ItemId, LookAhead = nTokenFirst};
									var nNewLr1ItemId = GetLr1ItemId(newLr1Item);
									if (!open.Contains(nNewLr1ItemId) && !closed.Contains(nNewLr1ItemId))
									{
										open.Add(nNewLr1ItemId);
									}
								}
							}

							nProduction++;
						}
					}
				}
			}

			return closed;
		}

		/// <summary>
		/// takes an LR0 state, and a tokenID, and produces the next state given the token and productions of the grammar
		/// </summary>
		private int GotoLr0(int nState, int nTokenId, ref bool bAdded, ref int nPrecedence)
		{
			var gotoLr0 = new HashSet<int>();
			var state = Lr0States[nState];
			foreach (var nItem in state)
			{
				var item = Lr0Items[nItem];
				if (item.Position < Productions[item.Production].Right.Length &&
				    (Productions[item.Production].Right[item.Position] == nTokenId))
				{
					var newItem = new Lr0Item {Production = item.Production, Position = item.Position + 1};
					var nNewItemId = GetLr0ItemId(newItem);
					gotoLr0.Add(nNewItemId);
					var nProductionPrecedence = ProductionPrecedence[item.Production];
					if (nPrecedence < nProductionPrecedence)
					{
						nPrecedence = nProductionPrecedence;
					}
				}
			}

			if (gotoLr0.Count == 0)
			{
				return -1;
			}
			else
			{
				return GetLr0StateId(Lr0Closure(gotoLr0), ref bAdded);
			}
		}

		/// <summary>
		/// Generates all of the LR 0 Items
		/// </summary>
		private void GenerateLr0Items()
		{
			var startState = new HashSet<int> {GetLr0ItemId(new Lr0Item {Production = 0, Position = 0})};

			var bIgnore = false;
			var open = new List<int> {GetLr0StateId(Lr0Closure(startState), ref bIgnore)};

			while (open.Count > 0)
			{
				var nState = open[0];
				open.RemoveAt(0);
				while (LrGoto.Count <= nState)
				{
					LrGoto.Add(new int [Grammar.Tokens.Length]);
					GotoPrecedence.Add(new int [Grammar.Tokens.Length]);
				}

				for (var nToken = 0; nToken < Grammar.Tokens.Length; nToken++)
				{
					var bAdded = false;
					var nPrecedence = int.MinValue;
					var nGoto = GotoLr0(nState, nToken, ref bAdded, ref nPrecedence);

					LrGoto[nState][nToken] = nGoto;
					GotoPrecedence[nState][nToken] = nPrecedence;

					if (bAdded)
					{
						open.Add(nGoto);
					}
				}
			}
		}


		/// <summary>
		/// Computes the set of first terminals for each token in the grammar
		/// </summary>
		private void ComputeFirstSets()
		{
			var nCountTokens = NonTerminals.Count + Terminals.Count;
			FirstSets = new HashSet<int> [nCountTokens];
			for (var nIdx = 0; nIdx < nCountTokens; nIdx++)
			{
				FirstSets[nIdx] = new HashSet<int>();
				if (Terminals.Contains(nIdx))
				{
					FirstSets[nIdx].Add(nIdx);
				}
			}

			foreach (var production in Productions)
			{
				if (production.Right.Length == 0)
				{
					FirstSets[production.Left].Add(-1);
				}
			}

			bool bDidSomething;
			do
			{
				bDidSomething = false;
				foreach (var production in Productions)
				{
					foreach (var nToken in production.Right)
					{
						var bLookAhead = false;
						foreach (var nTokenFirst in FirstSets[nToken])
						{
							if (nTokenFirst == -1)
							{
								bLookAhead = true;
							}
							else if (FirstSets[production.Left].Add(nTokenFirst))
							{
								bDidSomething = true;
							}
						}

						if (!bLookAhead)
						{
							break;
						}
					}
				}
			} while (bDidSomething);
		}

		/// <summary>
		/// returns the set of terminals that are possible to see next given an arbitrary list of tokens
		/// </summary>
		private HashSet<int> First(List<int> tokens, int nTerminal)
		{
			var first = new HashSet<int>();
			foreach (var nToken in tokens)
			{
				var bLookAhead = false;
				foreach (var nTokenFirst in FirstSets[nToken])
				{
					if (nTokenFirst == -1)
					{
						bLookAhead = true;
					}
					else
					{
						first.Add(nTokenFirst);
					}
				}

				if (!bLookAhead)
				{
					return first;
				}
			}

			first.Add(nTerminal);
			return first;
		}

		/// <summary>
		/// Initializes the propagation table, and initial state of the LALR table
		/// </summary>
		private void InitLalrTables()
		{
			var nLr0State = 0;
			foreach (var unused in Lr0States)
			{
				LalrStates.Add(new HashSet<int>());
			}

			foreach (var lr0Kernel in Lr0Kernels)
			{
				var j = new HashSet<int>();
				foreach (var jLr0ItemId in lr0Kernel)
				{
					var lr1Item = new Lr1Item {Lr0ItemId = jLr0ItemId, LookAhead = -1};
					var nLr1ItemId = GetLr1ItemId(lr1Item);
					j.Add(nLr1ItemId);
				}

				var jPrime = Lr1Closure(j);
				foreach (var jpLr1ItemId in jPrime)
				{
					var lr1Item = Lr1Items[jpLr1ItemId];
					var lr0Item = Lr0Items[lr1Item.Lr0ItemId];

					if ((lr1Item.LookAhead != -1) || (nLr0State == 0))
					{
						LalrStates[nLr0State].Add(jpLr1ItemId);
					}

					if (lr0Item.Position < Productions[lr0Item.Production].Right.Length)
					{
						var nToken = Productions[lr0Item.Production].Right[lr0Item.Position];
						var lr0Successor = new Lr0Item
							{Production = lr0Item.Production, Position = lr0Item.Position + 1};
						var nLr0Successor = GetLr0ItemId(lr0Successor);
						var nSuccessorState = LrGoto[nLr0State][nToken];
						if (lr1Item.LookAhead == -1)
						{
							AddPropagation(nLr0State, lr1Item.Lr0ItemId, nSuccessorState, nLr0Successor);
						}
						else
						{
							var lalrItem = new Lr1Item {Lr0ItemId = nLr0Successor, LookAhead = lr1Item.LookAhead};
							var nLalrItemId = GetLr1ItemId(lalrItem);
							LalrStates[nSuccessorState].Add(nLalrItemId);
						}
					}
				}

				nLr0State++;
			}
		}

		/// <summary>
		/// Calculates the states in the LALR table
		/// </summary>
		private void CalculateLookAhead()
		{
			bool bChanged;
			do
			{
				bChanged = false;
				var nState = 0;
				foreach (var statePropagations in LalrPropagations)
				{
					var bStateChanged = false;
					foreach (var nLr1Item in LalrStates[nState])
					{
						var lr1Item = Lr1Items[nLr1Item];

						if (statePropagations.ContainsKey(lr1Item.Lr0ItemId))
						{
							foreach (var lalrPropagation in statePropagations[lr1Item.Lr0ItemId])
							{
								var nGoto = lalrPropagation.Lr0TargetState;
								var item = new Lr1Item
									{Lr0ItemId = lalrPropagation.Lr0TargetItem, LookAhead = lr1Item.LookAhead};
								if (LalrStates[nGoto].Add(GetLr1ItemId(item)))
								{
									bChanged = true;
									bStateChanged = true;
								}
							}
						}
					}

					if (bStateChanged)
					{
						LalrStates[nState] = Lr1Closure(LalrStates[nState]);
					}

					nState++;
				}
			} while (bChanged);
		}

		/// <summary>
		/// Initializes the tokens for the grammar
		/// </summary>
		private void InitSymbols()
		{
			for (var nSymbol = 0; nSymbol < Grammar.Tokens.Length; nSymbol++)
			{
				var bTerminal = true;
				foreach (var production in Productions)
				{
					if (production.Left == nSymbol)
					{
						bTerminal = false;
						break;
					}
				}

				if (bTerminal)
				{
					Terminals.Add(nSymbol);
				}
				else
				{
					NonTerminals.Add(nSymbol);
				}
			}
		}

		/// <summary>
		/// Converts an LR0 State to an LR0 Kernel consisting of only the 'initiating' LR0 Items in the state
		/// </summary>
		public void ConvertLr0ItemsToKernels()
		{
			foreach (var lr0State in Lr0States)
			{
				var lr0Kernel = new HashSet<int>();
				foreach (var nLr0Item in lr0State)
				{
					var item = Lr0Items[nLr0Item];
					if (item.Position != 0)
					{
						lr0Kernel.Add(nLr0Item);
					}
					else if (Productions[item.Production].Left == 0)
					{
						lr0Kernel.Add(nLr0Item);
					}
				}

				Lr0Kernels.Add(lr0Kernel);
			}
		}

		/// <summary>
		/// Helper function that returns true if the list of actions contains an action
		/// </summary>
		private bool ListContainsAction(List<Action> list, Action action)
		{
			foreach (var listAction in list)
			{
				if (listAction.Equals(action))
				{
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Generates the parse table given the lalr states, and grammar
		/// </summary>
		private void GenerateParseTable()
		{
			ParseTable = new ParseTable
			{
				Actions = new Action[LalrStates.Count, Grammar.Tokens.Length + 1]
			};
			for (var nStateId = 0; nStateId < LalrStates.Count; nStateId++)
			{
				var lalrState = LalrStates[nStateId];

				for (var nToken = -1; nToken < Grammar.Tokens.Length; nToken++)
				{
					var actions = new List<Action>();
					if (nToken >= 0)
					{
						if (LrGoto[nStateId][nToken] >= 0)
						{
							actions.Add(new Action
								{ActionType = ActionType.Shift, ActionParameter = LrGoto[nStateId][nToken]});
						}
					}

					foreach (var nLr1ItemId in lalrState)
					{
						var lr1Item = Lr1Items[nLr1ItemId];
						var lr0Item = Lr0Items[lr1Item.Lr0ItemId];

						if ((lr0Item.Position == Productions[lr0Item.Production].Right.Length) &&
						    lr1Item.LookAhead == nToken)
						{
							var action = new Action
								{ActionType = ActionType.Reduce, ActionParameter = lr0Item.Production};
							if (!ListContainsAction(actions, action))
							{
								actions.Add(action);
							}
						}
					}

					var nMaxPrecedence = int.MinValue;
					var importantActions = new List<Action>();
					foreach (var action in actions)
					{
						var nActionPrecedence = int.MinValue;
						if (action.ActionType == ActionType.Shift)
						{
							nActionPrecedence = GotoPrecedence[nStateId][nToken]; //nToken will never be -1
						}
						else if (action.ActionType == ActionType.Reduce)
						{
							nActionPrecedence = ProductionPrecedence[action.ActionParameter];
						}

						if (nActionPrecedence > nMaxPrecedence)
						{
							nMaxPrecedence = nActionPrecedence;
							importantActions.Clear();
							importantActions.Add(action);
						}
						else if (nActionPrecedence == nMaxPrecedence)
						{
							importantActions.Add(action);
						}
					}

					if (importantActions.Count == 1)
					{
						ParseTable.Actions[nStateId, nToken + 1] = importantActions[0];
					}
					else if (importantActions.Count > 1)
					{
						Action shiftAction = default(Action);
						var reduceActions = new List<Action>();
						foreach (var action in importantActions)
						{
							if (action.ActionType == ActionType.Reduce)
							{
								reduceActions.Add(action);
							}
							else
							{
								shiftAction = action;
							}
						}

						var derv = Grammar.PrecedenceGroups[-nMaxPrecedence].Derivation;
						if (derv == Derivation.LeftMost && reduceActions.Count == 1)
						{
							ParseTable.Actions[nStateId, nToken + 1] = reduceActions[0];
						}
						else if (derv == Derivation.RightMost && shiftAction != null)
						{
							ParseTable.Actions[nStateId, nToken + 1] = shiftAction;
						}
						else
						{
							if (derv == Derivation.None && reduceActions.Count == 1)
							{
								Console.WriteLine("Error, shift-reduce conflict in grammar");
							}
							else
							{
								Console.WriteLine("Error, reduce-reduce conflict in grammar");
							}

							ParseTable.Actions[nStateId, nToken + 1] = new Action
								{ActionType = ActionType.Error, ActionParameter = nToken};
						}
					}
					else
					{
						ParseTable.Actions[nStateId, nToken + 1] = new Action
							{ActionType = ActionType.Error, ActionParameter = nToken};
					}
				}
			}
		}

		/// <summary>
		/// helper function
		/// </summary>
		private void PopulateProductions()
		{
			var nPrecedence = 0;
			foreach (var oGroup in Grammar.PrecedenceGroups)
			{
				foreach (var oProduction in oGroup.Productions)
				{
					Productions.Add(oProduction);
					ProductionPrecedence.Add(nPrecedence);
					ProductionDerivation.Add(oGroup.Derivation);
				}

				nPrecedence--;
			}
		}

		/// <summary>
		/// constructor, construct parser table
		/// </summary>
		public Parser(Grammar grammar)
		{
			LrGoto = new List<int[]>();
			GotoPrecedence = new List<int[]>();
			Lr0Items = new List<Lr0Item>();
			Lr1Items = new List<Lr1Item>();
			Lr0States = new List<HashSet<int>>();
			Lr0Kernels = new List<HashSet<int>>();
			LalrStates = new List<HashSet<int>>();
			Terminals = new List<int>();
			NonTerminals = new List<int>();
			LalrPropagations = new List<Dictionary<int, List<LalrPropagation>>>();
			Grammar = grammar;
			Productions = new List<Production>();
			ProductionDerivation = new List<Derivation>();
			ProductionPrecedence = new List<int>();

			PopulateProductions();
			InitSymbols();
			GenerateLr0Items();
			ComputeFirstSets();
			ConvertLr0ItemsToKernels();
			InitLalrTables();
			CalculateLookAhead();
			GenerateParseTable();
		}
	}
}