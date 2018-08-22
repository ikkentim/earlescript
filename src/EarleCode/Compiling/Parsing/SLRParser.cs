using System;
using System.Collections.Generic;
using System.Linq;
using EarleCode.Compiling.Lexing;
using EarleCode.Compiling.Parsing.Grammars;
using EarleCode.Compiling.Parsing.Grammars.Productions;
using EarleCode.Compiling.Parsing.ParseTree;

namespace EarleCode.Compiling.Parsing
{
	/// <summary>
	/// Represents an SLR(1) parser.
	/// </summary>
	public class SLRParser : IParser
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="SLRParser"/> class.
		/// </summary>
		/// <param name="grammar">The grammar to build the parse table with.</param>
	    public SLRParser(IGrammar grammar)
	    {
		    Grammar = grammar ?? throw new ArgumentNullException(nameof(grammar));
		    Table = new SLRParsingTable(grammar);
	    }

		/// <summary>
		///     Gets the production rules used by this parser.
		/// </summary>
		public IGrammar Grammar { get; }
		
		/// <summary>
		///     Gets the parsing table used by this parser.
		/// </summary>
		public SLRParsingTable Table { get; }

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
		    var startRules = Grammar.Get(Grammar.Default).ToArray();
		    if (startRules.Length != 1)
			    throw new ParserException("Invalid default grammar rule.");
		    var startRule = startRules[0];

			stateStack.Push(Table.InitialState);

		    for (;;)
		    {
			    var token = index >= tokens.Length ? Token.EndOfFile : tokens[index];
			    var action = Table[stateStack.Peek(), new ProductionRuleElement(token.Type, token.Value)];
				
			    if (action.Type == SLRActionType.Error)
			    {
					action = Table[stateStack.Peek(), new ProductionRuleElement(token.Type, null)];
			    }
			    if (action.Type == SLRActionType.Error)
			    {
				    // TODO: Remove this (empty terminal is not in table) or not???
				    action = Table[stateStack.Peek(), new ProductionRuleElement(ProductionRuleElementType.TerminalEmpty, null)];

				    if (action.Type != SLRActionType.Error)
					    token = new Token(TokenFlag.Empty, TokenType.Symbol, null, token.File, token.Line, token.Column);
			    }

				switch (action.Type)
			    {
				    case SLRActionType.GoTo:
					    throw new ParserException("The parser table is messed up.");
				    case SLRActionType.Error:
					    throw new UnexpectedTokenException(token);
				    case SLRActionType.Shift:
					    symbolStack.Push(new LeafNode(token));
					    stateStack.Push(action.Value);
						
						if(!token.IsEmpty)
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

								    if (!element.Token.Describes(leafNode.Token))
									    throw new ParserException();
								    break;
						    }

						    childNodes[i] = symbol;
					    }

					    symbolStack.Push(new InteriorNode(rule.Name, childNodes));

					    if (rule == startRule)
					    {
						    // Accept;
						    return symbolStack.Pop();
					    }

					    var goToAction = Table[stateStack.Peek(), new ProductionRuleElement(ProductionRuleElementType.NonTerminal, rule.Name)];

					    if (goToAction.Type != SLRActionType.GoTo)
						    throw new ParserException();

					    stateStack.Push(goToAction.Value);

					    break;

			    }
		    }
	    }

	    #endregion
    }
}
