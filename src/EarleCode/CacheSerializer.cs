// EarleCode
// Copyright 2018 Tim Potze
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using EarleCode.Compiling.Lexing;
using EarleCode.Compiling.Parsing.Grammars.Productions;

namespace EarleCode.Compiling.Parsing.Grammars
{
	public class CacheSerializer
	{
		// NOTE: Ugly code, but does work
		private readonly byte[] _buffer = new byte[1024];
		private readonly byte[] _full = { 0xff };
		private readonly byte[] _zero = { 0x00 };
		private bool _didRead;
		private int _head;
		private int _tail;

		private void ResetBuffer()
		{
			_head = 0;
			_tail = 0;
		}

		private void Read(Stream stream)
		{
			if (_tail >= _buffer.Length)
				return;

			var read = stream.Read(_buffer, _tail, _buffer.Length - _tail);

			_didRead = read > 0;
			_tail += read;
		}

		private void Move()
		{
			if (_tail > _head)
				Array.Copy(_buffer, _head, _buffer, 0, _tail - _head);

			_tail -= _head;
			_head = 0;
		}

		private int StringLength(int offset = 0)
		{
			for (var i = 0; _head + offset + i < _tail; i++)
			{
				if (_buffer[_head + offset + i] == 0x00) return i;
			}

			return -1;
		}

		private bool ReadString(out string result, int offset = 0, int length = -1)
		{
			if (length < 0)
			{
				length = StringLength(offset);

				if (length < 0)
				{
					if (!_didRead)
						throw new EndOfStreamException();

					result = null;
					return false;
				}
			}

			result = length == 0 ? null : Encoding.ASCII.GetString(_buffer, _head + offset, length);

			_head += offset + length + 1;

			return true;
		}

		private bool Require(int length)
		{
			if (_tail - _head < length && !_didRead)
				throw new EndOfStreamException();

			return _tail - _head >= length;
		}

		public IGrammar DeserializeGrammar(Stream stream)
		{
			if (stream == null) throw new ArgumentNullException(nameof(stream));

			ResetBuffer();

			var state = GState.FindDefault;
			string symbolName = null;
			var elements = new List<ProductionRuleElement>();
			var grammar = new Grammar();

			while (state != GState.Done)
			{
				if (_buffer.Length - _tail < _buffer.Length / 4)
					Move();
				Read(stream);

				Require(1);

				switch (state)
				{
					case GState.FindDefault:
						if (!ReadString(out var str))
							break;
						grammar.Default = str;
						state = GState.FindSymbol;
						break;
					case GState.FindSymbol:
						if (_head != _tail && _buffer[_head] == 0x00)
						{
							state = GState.Done;
							break;
						}

						if (!ReadString(out symbolName))
							break;

						state = GState.FindRule;
						break;
					case GState.FindRule:
						if (_head != _tail && _buffer[_head] == 0xff)
						{
							state = GState.FindSymbol;
							_head++;
							break;
						}

						state = GState.FindElement;
						break;

					case GState.FindElement:
						if (!ParseElement(out var element))
						{
							grammar.Add(symbolName, new ProductionRule(symbolName, elements.ToArray()));
							elements.Clear();

							state = GState.FindRule;
						}

						elements.Add(element);
						break;
				}
			}

			return grammar;
		}

		private bool ParseElement(out ProductionRuleElement element)
		{
			// false when finished
			if (_head != _tail && _buffer[_head] == 0xff)
			{
				_head++;
				element = default(ProductionRuleElement);
				return false;
			}

			if (!Require(2))
			{
				element = default(ProductionRuleElement);
				return true;
			}

			var tokenType = (TokenType) _buffer[_head];
			var elementType = (ProductionRuleElementType) _buffer[_head + 1];

			if (!ReadString(out var str, 2))
			{
				element = default(ProductionRuleElement);
				return true;
			}

			element = new ProductionRuleElement(elementType, tokenType, str);
			return true;
		}

		private void Serialize(ProductionRuleElement element, Stream destination, StreamWriter writer)
		{
			_buffer[0] = (byte) element.TokenType;
			_buffer[1] = (byte) element.Type;
			destination.Write(_buffer, 0, 2);

			if (element.Value != null)
				writer.Write(element.Value);

			destination.Write(_zero, 0, 1);
		}

		private void Serialize(ProductionRule rule, Stream destination, StreamWriter writer, bool includeNames = false)
		{
			if (includeNames)
				writer.Write(rule.Name);

			destination.Write(_zero, 0, 1);

			foreach (var element in rule.Elements)
			{
				Serialize(element, destination, writer);
			}

			destination.Write(_full, 0, 1);
		}

		private void Serialize(IEnumerable<ProductionRule> rules, Stream destination, StreamWriter writer, bool includeNames = false)
		{
			foreach (var rule in rules)
			{
				Serialize(rule, destination, writer, includeNames);
			}

			destination.Write(_full, 0, 1);
		}

		public void Serialize(IGrammar grammar, Stream destination)
		{
			if (grammar == null) throw new ArgumentNullException(nameof(grammar));
			if (destination == null) throw new ArgumentNullException(nameof(destination));

			using (var writer = new StreamWriter(destination, Encoding.ASCII, 1024, true))
			{
				writer.AutoFlush = true;

				if (grammar.Default != null)
					writer.Write(grammar.Default);

				destination.Write(_zero, 0, 1);

				foreach (var symbol in grammar.Symbols)
				{
					writer.Write(symbol);
					destination.Write(_zero, 0, 1);

					Serialize(grammar.Get(symbol), destination, writer);
				}

				destination.Write(_zero, 0, 1);
			}
		}

		private void Serialize(SLRAction action, Stream destination, ProductionRule[] rules)
		{
			_buffer[0] = (byte) action.Type;
			destination.Write(_buffer, 0, 1);

			if (action.Type == SLRActionType.Reduce)
			{
				var index = Array.IndexOf(rules, action.Reduce);
				if (index < 0)
					throw new Exception("Serialization error");

				destination.Write(BitConverter.GetBytes(index), 0, 4);
			}
			else
				destination.Write(BitConverter.GetBytes(action.Value), 0, 4);
		}

		public void Serialize(SLRParsingTable table, Stream destination)
		{
			if (table == null) throw new ArgumentNullException(nameof(table));
			if (destination == null) throw new ArgumentNullException(nameof(destination));

			using (var writer = new StreamWriter(destination, Encoding.ASCII, 1024, true))
			{
				writer.AutoFlush = true;

				var rules = Enumerable.Range(0, table.States)
					.Select(table.GetRow)
					.SelectMany(x => x.Values)
					.Select(x => x.Reduce)
					.Distinct()
					.Where(x => x != null)
					.ToArray();
				
				var defaultIndex = Array.IndexOf(rules, table.Default);
				if (defaultIndex < 0)
					throw new Exception("Serialization error");

				destination.Write(BitConverter.GetBytes(table.InitialState), 0, 4);
				destination.Write(BitConverter.GetBytes(table.States), 0, 4);
				destination.Write(BitConverter.GetBytes(defaultIndex), 0, 4);
				Serialize(rules, destination, writer, true);

				for (var state = 0; state < table.States; state++)
				{
					var row = table.GetRow(state);

					destination.Write(BitConverter.GetBytes(row.Count), 0, 4);

					foreach (var kv in row)
					{
						Serialize(kv.Key, destination, writer);
						Serialize(kv.Value, destination, rules);
					}
				}
			}
		}

		public SLRParsingTable DeserializeSLRParsingTable(Stream stream)
		{
			if (stream == null) throw new ArgumentNullException(nameof(stream));

			ResetBuffer();

			var state = SState.FindInitial;
			var initialState = -1;
			var stateCount = -1;
			var rules = new List<ProductionRule>();
			var elements = new List<ProductionRuleElement>();
			string ruleName = null;
			var currentState = 0;
			var elementCount = 0;
			var currentElement = 0;
			var defaultIndex = 0;
			var stateElement = default(ProductionRuleElement);
			var result = new List<Dictionary<ProductionRuleElement, SLRAction>>();

			while (state != SState.Done)
			{
				if (_buffer.Length - _tail < _buffer.Length / 4)
					Move();

				Read(stream);

				switch (state)
				{
					case SState.FindInitial:
						if (!Require(12))
							break;

						initialState = BitConverter.ToInt32(_buffer, _head);
						stateCount = BitConverter.ToInt32(_buffer, _head + 4);
						defaultIndex = BitConverter.ToInt32(_buffer, _head + 8);
						_head += 12;
						state = SState.FindRule;
						break;
					case SState.FindRule:
						if (_head != _tail && _buffer[_head] == 0xff)
						{
							_head++;
							state = SState.FindStateSize;
							break;
						}

						if (ReadString(out ruleName))
							state = SState.FindRuleElement;
						break;
					case SState.FindRuleElement:
						if (!ParseElement(out var element))
						{
							rules.Add(new ProductionRule(ruleName, elements.ToArray()));
							elements.Clear();
							state = SState.FindRule;
							break;
						}

						elements.Add(element);

						break;
					case SState.FindStateSize:
						if (currentState >= stateCount)
						{
							state = SState.Done;
							break;
						}

						if (!Require(4))
							break;

						result.Add(new Dictionary<ProductionRuleElement, SLRAction>());
						elementCount = BitConverter.ToInt32(_buffer, _head);
						currentElement = 0;
						state = SState.FindStateElement;

						_head += 4;
						break;
					case SState.FindStateElement:
						if (currentElement >= elementCount)
						{
							currentState++;
							state = SState.FindStateSize;
							break;
						}

						if (ParseElement(out stateElement))
							state = SState.FindStateAction;

						break;
					case SState.FindStateAction:
						if (!Require(5))
							break;

						var type = (SLRActionType) _buffer[_head];
						_head++;

						var value = BitConverter.ToInt32(_buffer, _head);
						_head += 4;

						var action = type == SLRActionType.Reduce
							? new SLRAction(rules[value])
							: new SLRAction(value, type);

						result.Last()[stateElement] = action;
						currentElement++;
						state = SState.FindStateElement;

						break;
				}
			}

			return new SLRParsingTable(initialState, result, rules[defaultIndex]);
		}

		private enum GState
		{
			FindDefault,
			FindSymbol,
			FindRule,
			FindElement,
			Done
		}

		private enum SState
		{
			Done,
			FindInitial,
			FindRule,
			FindRuleElement,
			FindStateSize,
			FindStateElement,
			FindStateAction
		}
	}
}