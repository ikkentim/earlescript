using System;
using System.Collections.Generic;
using EarleCode.Compiling.Parsing.Grammars;
using EarleCode.Compiling.Parsing.Grammars.Productions;

namespace EarleCode.Compiling.Parsing
{
	public class ShiftReduceParsingTable
	{
		private readonly List<Dictionary<Terminal, SLRAction>> _actions = new List<Dictionary<Terminal, SLRAction>>();
		private readonly List<Dictionary<string, int>> _goTos = new List<Dictionary<string, int>>();

		/// <summary>
		/// Gets or sets the number of states.
		/// </summary>
		public int States
		{
			get => _actions.Count;
			set
			{
				if(value < 0)
					throw new ArgumentOutOfRangeException(nameof(value));

				if (value < _actions.Count)
				{
					_actions.RemoveRange(value, _actions.Count - value);
					_goTos.RemoveRange(value, _goTos.Count - value);
				}
				else
				{
					for (var i = _actions.Count; i < value; i++)
					{
						_actions.Add(new Dictionary<Terminal, SLRAction>());
						_goTos.Add(new Dictionary<string, int>());
					}
				}
			}
		}

		public bool TryGetAction(int state, Terminal terminal, out SLRAction action)
		{
			if (state < 0 || state >= _actions.Count)
				throw new ArgumentOutOfRangeException(nameof(state), state, "Value should be between 0 and States");
			return _actions[state].TryGetValue(terminal, out action);
		}

		public bool TryGetGoTo(int state, string nonTerminal, out int goTo)
		{
			if (state < 0 || state >= _actions.Count)
				throw new ArgumentOutOfRangeException(nameof(state), state, "Value should be between 0 and States");
			return _goTos[state].TryGetValue(nonTerminal, out goTo);
		}

		public SLRAction this[int state, Terminal terminal]
		{
			get
			{
				if (state < 0 || state >= _actions.Count)
					throw new ArgumentOutOfRangeException(nameof(state), state, "Value should be between 0 and States");
				return _actions[state].TryGetValue(terminal, out var value) ? value : SLRAction.Error;
			}
			set
			{
				if (state < 0 || state >= _actions.Count)
					throw new ArgumentOutOfRangeException(nameof(state), state, "Value should be between 0 and States");
				_actions[state][terminal] = value;
			}
		}

		/// <summary>
		/// Gets or sets the GOTO value for the specified non-terminal. Returns -1 if no GOTO value is found.
		/// </summary>
		/// <param name="state"></param>
		/// <param name="nonTerminal"></param>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		public int this[int state, string nonTerminal]
		{
			get
			{
				if (state < 0 || state >= _actions.Count)
					throw new ArgumentOutOfRangeException(nameof(state), state, "Value should be between 0 and States");
				return _goTos[state].TryGetValue(nonTerminal, out var value) ? value : -1;
			}
			set
			{
				if (state < 0 || state >= _actions.Count)
					throw new ArgumentOutOfRangeException(nameof(state), state, "Value should be between 0 and States");
				_goTos[state][nonTerminal] = value;
			}
		}

		public ProductionRule Default { get; set; }

		public int InitialState { get; set; } = -1;
	}
}