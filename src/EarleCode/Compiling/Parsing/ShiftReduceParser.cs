using System;
using System.Collections.Generic;
using EarleCode.Compiling.Lexing;
using EarleCode.Compiling.Parsing.Grammars;
using EarleCode.Compiling.Parsing.Grammars.Productions;
using EarleCode.Compiling.Parsing.ParseTree;

namespace EarleCode.Compiling.Parsing
{
	/// <summary>
	/// Represents a shift-reduce parser.
	/// </summary>
	public class ShiftReduceParser : IParser
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ShiftReduceParser"/> class.
		/// </summary>
		/// <param name="table">The table to use while parsing.</param>
		public ShiftReduceParser(ShiftReduceParsingTable table)
		{
			Table = table ?? throw new ArgumentNullException(nameof(table));
		}

		/// <summary>
		///     Gets the parsing table used by this parser.
		/// </summary>
		public ShiftReduceParsingTable Table { get; }

		#region Implementation of IParser

		/// <summary>
		///     Parses the specified tokens into a parse tree.
		/// </summary>
		/// <param name="tokens">The tokens to parse.</param>
		/// <returns>The parse tree.</returns>
		/// <exception cref="ParserException">Thrown if an error occurs while parsing the specified tokens.</exception>
		public INode Parse(Token[] tokens)
		{
			if (tokens == null) throw new ArgumentNullException(nameof(tokens));
			
			var stateStack = new Stack<int>();
			var symbolStack = new Stack<INode>();
			var index = 0;

			var startRule = Table.Default;

			stateStack.Push(Table.InitialState);

			for (;;)
			{
				var isEof = index >= tokens.Length;
				var token = isEof ? default(Token) : tokens[index];
				var terminal = isEof ? Terminal.EndOfFile : new Terminal(tokens[index].Type, tokens[index].Value);
                
				var action = Table[stateStack.Peek(), terminal];
				
				if (action.Type == SLRActionType.Error && terminal.Type == TerminalType.Default)
				{
					action = Table[stateStack.Peek(), new Terminal(terminal.TokenType, null)];
				}
                
				switch (action.Type)
				{
					case SLRActionType.Error:
						if(isEof)
							throw new ParserException("Unexpected end of file.");
						throw new UnexpectedTokenException(token);
					case SLRActionType.Shift:
						if (isEof)
							throw new ParserException("Unexpected end of file.");
						symbolStack.Push(new LeafNode(token));
						stateStack.Push(action.Value);

						index++;
						break;
					case SLRActionType.Reduce:
						var rule = action.Reduce;

						var childNodes = new INode[rule.Elements.Length];

						for (var i = childNodes.Length - 1; i >= 0; i--)
						{
							var symbol = symbolStack.Pop();
							stateStack.Pop();
							var element = rule.Elements[i];

							switch (element.Type)
							{
								case ProductionRuleElementType.NonTerminal:
									if (!(symbol is IInteriorNode interiorNode))
										throw new ParserException();

									if (interiorNode.Rule != element.Value)
										throw new ParserException();
									break;
								case ProductionRuleElementType.Terminal:
									if (!(symbol is ILeafNode leafNode))
										throw new ParserException();

									if (!element.Terminal.Describes(new Terminal(leafNode.Token.Type, leafNode.Token.Value)))
										throw new ParserException();
									break;
							}

							childNodes[i] = symbol;
						}

						symbolStack.Push(new InteriorNode(rule.Name, childNodes));

						if (rule == startRule)
						{
							// Accept
							return symbolStack.Pop();
						}

						var goTo = Table[stateStack.Peek(), rule.Name];

						if(goTo < 0)
							throw new ParserException();
                        
						stateStack.Push(goTo);

						break;
					default:
						throw new ParserException();
				}
			}
		}

		#endregion
	}
}