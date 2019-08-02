using System;
using System.Collections.Generic;
using System.Linq;
using EarleCode.Compiling.Earle;
using EarleCode.Compiling.Lexing;
using EarleCode.Compiling.Parsing.CodeProject;
using EarleCode.Compiling.Parsing.Grammars;
using EarleCode.Compiling.Parsing.Grammars.Productions;
using EarleCode.Compiling.Parsing.ParseTree;

namespace EarleCode.Compiling.Parsing
{
    public class LALRParser : IParser
    {
	    private readonly Parser _parser;
	    private readonly string[] _tokens;
	    
	    public LALRParser(IGrammar grammar)
	    {
		    // Temporary wrapper while I build my own LALR parser... Only works for Earle's ProductionRuleElementType grammar.
		    var def = grammar.Default;
		    var nonTerminals = grammar.NonTerminals.Where(x => x != def).ToArray();
		    var terminals = grammar.All.SelectMany(x =>
				    x.Elements.Where(y =>
						    y.Type == ProductionRuleElementType.Terminal && 
						    (y.TokenType == TokenType.Symbol ||
						    y.TokenType == TokenType.Keyword))
					    .Select(y => y.Value))
			    .Concat(new[] {"Identifier", "StringLiteral", "NumberLiteral"})
			    .Distinct();

		    _tokens = new[] { def }
			    .Concat(nonTerminals)
			    .Concat(terminals)
			    .ToArray();

		     _parser = new Parser(ConvertGrammar(grammar, _tokens));
	    }

	    private static CodeProject.Grammar ConvertGrammar(IGrammar grammar, string[] tokens)
	    {
		    var result = new CodeProject.Grammar();
		    var names = Enum.GetNames(typeof(ProductionRuleEnum));

		    result.PrecedenceGroups = names.Select(name => new PrecedenceGroup
		    {
			    Derivation = name == "StatementIf" ? Derivation.RightMost : Derivation.None,
			    Productions = grammar.Get(name)
				    .Select(r =>
				    {
					    var x =  new Production
					    {
						    Left = Array.IndexOf(tokens, r.Name),
						    Right = r.Elements.Select(e => Array.IndexOf(tokens, e.TokenType == TokenType.Keyword || e.TokenType == TokenType.Symbol ? e.Value : e.TokenType.ToString())).ToArray()
					    };

					    if(x.Right.Any(z => z == -1)) throw new Exception("token error");
					    return x;
				    })
				    .ToArray()
		    }).ToArray();
		    result.Tokens = tokens;
		    
		    return result;
	    }

	    public INode Parse(Token[] tokens)
	    {
		    return Parse(tokens, _tokens, _parser);
	    }
        
		private static INode Parse(IReadOnlyList<Token> tokens, string[] tokenNames, Parser parser)
		{
			const int initState = 0;
			var tokenStack = new Stack<INode>();
			var stateStack = new Stack<int>();

			stateStack.Push(initState);
			var ptr = 0;

			while (true)
			{
				var token = ptr == tokens.Count ? default(Token): tokens[ptr];

				var tokenId = -1;

				if(ptr < tokens.Count)
					switch (token.Type)
					{
						case TokenType.Identifier:
							tokenId = Array.IndexOf(tokenNames, "Identifier");
							break;
						case TokenType.NumberLiteral:
							tokenId = Array.IndexOf(tokenNames, "NumberLiteral");
							break;
						case TokenType.StringLiteral:
							tokenId = Array.IndexOf(tokenNames, "StringLiteral");
							break;
						case TokenType.Keyword:
						case TokenType.Symbol:
							tokenId = Array.IndexOf(tokenNames, token.Value);
							break;
					}

				var action = parser.ParseTable.Actions[stateStack.Peek(), tokenId + 1];

				switch (action.ActionType)
				{
					case ActionType.Shift:
						if(tokenId == -1) // eof
							throw new ParserException("Unexpected end of file.");
						stateStack.Push(action.ActionParameter);
						tokenStack.Push(new LeafNode(token));
						ptr++;
						break;

					case ActionType.Reduce:
						var nProduction = action.ActionParameter;
						var production = parser.Productions[nProduction];
						var nChildren = production.Right.Length;
						INode reduction;
					
						var children = new INode[nChildren];
						for (var i = 0; i < nChildren; i++)
						{
							children[nChildren - i - 1] = tokenStack.Pop();
							stateStack.Pop();
						}

						reduction = new InteriorNode(tokenNames[production.Left], children);

						if (production.Left == 0)
						{
							return reduction;
						}

						tokenStack.Push(reduction);

						var state = parser.ParseTable.Actions[stateStack.Peek(), production.Left+1].ActionParameter;
						
						stateStack.Push(state);
						
						break;

					case ActionType.Error:
						throw new Exception("Error");
				}
			}
		}
    }
}