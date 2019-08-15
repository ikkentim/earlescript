using System;
using System.Collections.Generic;

namespace EarleCode.Compiling.Parsing.CodeProject
{
	public class Parser
	{
		private HashSet<int>[] _firstSets;
		private readonly List<Lr0Item> _lr0Items = new List<Lr0Item>();
		private readonly Dictionary<Lr0Item, int> _lr0Id = new Dictionary<Lr0Item, int>();
		private readonly List<Lr1Item> _lr1Items = new List<Lr1Item>();
		private readonly Dictionary<Lr1Item, int> _lr1Id = new Dictionary<Lr1Item, int>();
		private readonly List<HashSet<int>> _lr0States = new List<HashSet<int>>();
		private readonly List<HashSet<int>> _lr0Kernels = new List<HashSet<int>>();
		private readonly List<HashSet<Lr1Item>> _lalrStates = new List<HashSet<Lr1Item>>();
		private readonly List<int[]> _lrGoto = new List<int[]>();
		private readonly List<int[]> _gotoPrecedence = new List<int[]>();
		private readonly List<int> _terminals = new List<int>();
		private readonly List<int> _nonTerminals = new List<int>();
		private readonly List<Dictionary<Lr0Item, List<LalrPropagation>>> _lalrPropagations = new List<Dictionary<Lr0Item, List<LalrPropagation>>>();
		private readonly List<int> _productionPrecedence = new List<int>();
		private readonly Grammar _grammar;

		public List<Production> Productions { get; } = new List<Production>();

		public ParseTable ParseTable { get; private set; }


		/// <summary>
		/// Adds a propagation to the propagation table
		/// </summary>
		private void AddPropagation(int nLr0SourceState, Lr0Item nLr0SourceItem, int nLr0TargetState, Lr0Item nLr0TargetItem)
		{
			while (_lalrPropagations.Count <= nLr0SourceState)
			{
				_lalrPropagations.Add(new Dictionary<Lr0Item, List<LalrPropagation>>());
			}

			var propagationsForState = _lalrPropagations[nLr0SourceState];
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
			if (_lr0Id.TryGetValue(item, out var id))
				return id;

			id = _lr0Items.Count;
			_lr0Items.Add(item);
			_lr0Id[item] = id;
			return id;
		}


		/// <summary>
		/// Gets the ID for a particular LR1 Item
		/// </summary>
		private int GetLr1ItemId(Lr1Item item)
		{
			if (_lr1Id.TryGetValue(item, out var id))
				return id;

			id = _lr1Items.Count;
			_lr1Items.Add(item);
			_lr1Id[item] = id;
			return id;
		}


		/// <summary>
		/// Gets the ID for a particular LR0 State
		/// </summary>
		private int GetLr0StateId(HashSet<int> state, ref bool bAdded)
		{
			var nStateId = 0;
			foreach (var oState in _lr0States)
			{
				if (oState.SetEquals(state))
				{
					return nStateId;
				}

				nStateId++;
			}

			_lr0States.Add(state);
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

			open.AddRange(i);

			while (open.Count > 0)
			{
				var nItem = open[0];
				open.RemoveAt(0);
				var item = _lr0Items[nItem];
				closed.Add(nItem);

				foreach (var production in Productions)
				{
					if ((item.Position < item.Production.Right.Length) &&
					    (production.Left == item.Production.Right[item.Position]))
					{
						var newItem = new Lr0Item(0, production);
						var nNewItemId = GetLr0ItemId(newItem);
						if (!open.Contains(nNewItemId) && !closed.Contains(nNewItemId))
						{
							open.Add(nNewItemId);
						}
					}
				}
			}

			return closed;
		}

		/// <summary>
		/// takes a set of LR1 Items (LR0 items with look-ahead) and produces all of those LR1 items reachable by substitution
		/// </summary>
		private HashSet<Lr1Item> Lr1Closure(HashSet<Lr1Item> i)
		{
			var closed = new HashSet<Lr1Item>();
			var open = new List<Lr1Item>();

			open.AddRange(i);

			while (open.Count > 0)
			{
				var lr1Item = open[0];
				open.RemoveAt(0);
				var lr0Item = lr1Item.Lr0ItemId;
				closed.Add(lr1Item);

				if (lr0Item.Position < lr0Item.Production.Right.Length)
				{
					var nToken = lr0Item.Production.Right[lr0Item.Position];
					if (_nonTerminals.Contains(nToken))
					{
						var argFirst = new List<int>();
						for (var nIdx = lr0Item.Position + 1;
							nIdx < lr0Item.Production.Right.Length;
							nIdx++)
						{
							argFirst.Add(lr0Item.Production.Right[nIdx]);
						}

						var first = First(argFirst, lr1Item.LookAhead);
						foreach (var production in Productions)
						{
							if (production.Left == nToken)
							{
								foreach (var nTokenFirst in first)
								{
									var newLr0Item = new Lr0Item(0, production);
									var newLr1Item = new Lr1Item {Lr0ItemId = newLr0Item, LookAhead = nTokenFirst};
									if (!open.Contains(newLr1Item) && !closed.Contains(newLr1Item))
									{
										open.Add(newLr1Item);
									}
								}
							}
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
			var state = _lr0States[nState];
			foreach (var nItem in state)
			{
				var item = _lr0Items[nItem];
				if (item.Position < item.Production.Right.Length &&
				    (item.Production.Right[item.Position] == nTokenId))
				{
					var newItem = new Lr0Item(item.Position + 1, item.Production);
					var nNewItemId = GetLr0ItemId(newItem);
					gotoLr0.Add(nNewItemId);
					var nProductionPrecedence = _productionPrecedence[item.Production.Id];
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
			var startState = new HashSet<int> {GetLr0ItemId(new Lr0Item(0, Productions[0]))};

			var bIgnore = false;
			var open = new List<int> {GetLr0StateId(Lr0Closure(startState), ref bIgnore)};

			while (open.Count > 0)
			{
				var nState = open[0];
				open.RemoveAt(0);
				while (_lrGoto.Count <= nState)
				{
					_lrGoto.Add(new int [_grammar.Tokens.Length]);
					_gotoPrecedence.Add(new int [_grammar.Tokens.Length]);
				}

				for (var nToken = 0; nToken < _grammar.Tokens.Length; nToken++)
				{
					var bAdded = false;
					var nPrecedence = int.MinValue;
					var nGoto = GotoLr0(nState, nToken, ref bAdded, ref nPrecedence);

					_lrGoto[nState][nToken] = nGoto;
					_gotoPrecedence[nState][nToken] = nPrecedence;

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
			var nCountTokens = _nonTerminals.Count + _terminals.Count;
			_firstSets = new HashSet<int> [nCountTokens];
			for (var nIdx = 0; nIdx < nCountTokens; nIdx++)
			{
				_firstSets[nIdx] = new HashSet<int>();
				if (_terminals.Contains(nIdx))
				{
					_firstSets[nIdx].Add(nIdx);
				}
			}

			foreach (var production in Productions)
			{
				if (production.Right.Length == 0)
				{
					_firstSets[production.Left].Add(-1);
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
						foreach (var nTokenFirst in _firstSets[nToken])
						{
							if (nTokenFirst == -1)
							{
								bLookAhead = true;
							}
							else if (_firstSets[production.Left].Add(nTokenFirst))
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
				foreach (var nTokenFirst in _firstSets[nToken])
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
			foreach (var unused in _lr0States)
			{
				_lalrStates.Add(new HashSet<Lr1Item>());
			}

			foreach (var lr0Kernel in _lr0Kernels)
			{
				var j = new HashSet<Lr1Item>();
				foreach (var jLr0ItemId in lr0Kernel)
				{
					var lr1Item = new Lr1Item {Lr0ItemId = _lr0Items[jLr0ItemId], LookAhead = -1};
					j.Add(lr1Item);
				}

				var jPrime = Lr1Closure(j);
				foreach (var jpLr1ItemId in jPrime)
				{
					var lr1Item = jpLr1ItemId;
					var lr0Item = lr1Item.Lr0ItemId;

					if ((lr1Item.LookAhead != -1) || (nLr0State == 0))
					{
						_lalrStates[nLr0State].Add(jpLr1ItemId);
					}

					if (lr0Item.Position < lr0Item.Production.Right.Length)
					{
						var nToken = lr0Item.Production.Right[lr0Item.Position];
						var lr0Successor = new Lr0Item(lr0Item.Position + 1, lr0Item.Production);
						var nSuccessorState = _lrGoto[nLr0State][nToken];
						if (lr1Item.LookAhead == -1)
						{
							AddPropagation(nLr0State, lr1Item.Lr0ItemId, nSuccessorState, lr0Successor);
						}
						else
						{
							var lalrItem = new Lr1Item {Lr0ItemId = lr0Successor, LookAhead = lr1Item.LookAhead};
							_lalrStates[nSuccessorState].Add(lalrItem);
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
				foreach (var statePropagations in _lalrPropagations)
				{
					var bStateChanged = false;
					foreach (var lr1Item in _lalrStates[nState])
					{
						if (statePropagations.ContainsKey(lr1Item.Lr0ItemId))
						{
							foreach (var lalrPropagation in statePropagations[lr1Item.Lr0ItemId])
							{
								var nGoto = lalrPropagation.Lr0TargetState;
								var item = new Lr1Item
									{Lr0ItemId = lalrPropagation.Lr0TargetItem, LookAhead = lr1Item.LookAhead};
								if (_lalrStates[nGoto].Add(item))
								{
									bChanged = true;
									bStateChanged = true;
								}
							}
						}
					}

					if (bStateChanged)
					{
						_lalrStates[nState] = Lr1Closure(_lalrStates[nState]);
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
			for (var nSymbol = 0; nSymbol < _grammar.Tokens.Length; nSymbol++)
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
					_terminals.Add(nSymbol);
				}
				else
				{
					_nonTerminals.Add(nSymbol);
				}
			}
		}

		/// <summary>
		/// Converts an LR0 State to an LR0 Kernel consisting of only the 'initiating' LR0 Items in the state
		/// </summary>
		private void ConvertLr0ItemsToKernels()
		{
			foreach (var lr0State in _lr0States)
			{
				var lr0Kernel = new HashSet<int>();
				foreach (var nLr0Item in lr0State)
				{
					var item = _lr0Items[nLr0Item];
					if (item.Position != 0)
					{
						lr0Kernel.Add(nLr0Item);
					}
					else if (item.Production.Left == 0)
					{
						lr0Kernel.Add(nLr0Item);
					}
				}

				_lr0Kernels.Add(lr0Kernel);
			}
		}

		/// <summary>
		/// Generates the parse table given the lalr states, and grammar
		/// </summary>
		private void GenerateParseTable()
		{
			ParseTable = new ParseTable
			{
				Actions = new Action[_lalrStates.Count, _grammar.Tokens.Length + 1]
			};

			var actions = new List<Action>();
			var importantActions = new List<Action>();
			var reduceActions = new List<Action>();

			for (var nStateId = 0; nStateId < _lalrStates.Count; nStateId++)
			{
				var lalrState = _lalrStates[nStateId];

				for (var nToken = -1; nToken < _grammar.Tokens.Length; nToken++)
				{
					actions.Clear();
					if (nToken >= 0)
					{
						if (_lrGoto[nStateId][nToken] >= 0)
						{
							actions.Add(new Action
								{ActionType = ActionType.Shift, ActionParameter = _lrGoto[nStateId][nToken]});
						}
					}

					foreach (var nLr1ItemId in lalrState)
					{
						var lr1Item = nLr1ItemId;
						var lr0Item = lr1Item.Lr0ItemId;

						if ((lr0Item.Position == lr0Item.Production.Right.Length) &&
						    lr1Item.LookAhead == nToken)
						{
							var action = new Action
								{ActionType = ActionType.Reduce, ActionParameter = lr0Item.Production.Id};
							if (!actions.Contains(action))
							{
								actions.Add(action);
							}
						}
					}

					var nMaxPrecedence = int.MinValue;
					importantActions.Clear();

					foreach (var action in actions)
					{
						var nActionPrecedence = int.MinValue;
						if (action.ActionType == ActionType.Shift)
						{
							nActionPrecedence = _gotoPrecedence[nStateId][nToken]; //nToken will never be -1
						}
						else if (action.ActionType == ActionType.Reduce)
						{
							nActionPrecedence = _productionPrecedence[action.ActionParameter];
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
						var shiftAction = default(Action);
						var hasShiftAction = false;
						reduceActions.Clear();
						foreach (var action in importantActions)
						{
							if (action.ActionType == ActionType.Reduce)
							{
								reduceActions.Add(action);
							}
							else
							{
								shiftAction = action;
								hasShiftAction = true;
							}
						}

						var derv = _grammar.PrecedenceGroups[-nMaxPrecedence].Derivation;
						if (derv == Derivation.LeftMost && reduceActions.Count == 1)
						{
							ParseTable.Actions[nStateId, nToken + 1] = reduceActions[0];
						}
						else if (derv == Derivation.RightMost && hasShiftAction)
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
			var nProduction = 0;
			foreach (var oGroup in _grammar.PrecedenceGroups)
			{
				foreach (var oProduction in oGroup.Productions)
				{
					oProduction.Id = nProduction++;
					Productions.Add(oProduction);
					_productionPrecedence.Add(nPrecedence);
				}

				nPrecedence--;
			}
		}

		/// <summary>
		/// constructor, construct parser table
		/// </summary>
		public Parser(Grammar grammar)
		{
			_grammar = grammar;

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