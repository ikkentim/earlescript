using System;
using System.Collections.Generic;
using EarleCode.Compiling.Lexing;
using EarleCode.Compiling.Parsing.Grammars;
using EarleCode.Compiling.Parsing.Grammars.Productions;
using EarleCode.Compiling.Parsing.ParseTree;

namespace EarleCode.Compiling.Parsing
{
    /// <summary>
    ///     Represents a basic LL(1) parser.
    /// </summary>
    public class LL1Parser : IParser
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="LL1Parser" /> class.
        /// </summary>
        /// <param name="grammar">The production rules.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="grammar" /> is null.</exception>
        public LL1Parser(IGrammar grammar)
        {
            Grammar = grammar ?? throw new ArgumentNullException(nameof(grammar));
            Table = new LL1ParsingTable(grammar);
        }

        /// <summary>
        ///     Gets the production rules used by this parser.
        /// </summary>
        public IGrammar Grammar { get; }

        /// <summary>
        ///     Gets the parsing table used by this parser.
        /// </summary>
        public LL1ParsingTable Table { get; }

        #region Implementation of IParser

        /// <summary>
        ///     Parses the specified tokens into a parse tree.
        /// </summary>
        /// <param name="tokens">The tokens to parse.</param>
        /// <returns>The parse tree.</returns>
        /// <exception cref="ParserException">Thrown if an error occurs while parsing the specified tokens.</exception>
        public INode Parse(Token[] tokens)
        {
            var index = 0;
            return Parse(Grammar.Default, tokens, ref index);
        }

        #endregion

        /// <summary>
        ///     Parses the specified tokens with the specified rule.
        /// </summary>
        /// <param name="rule">The symbol of the rule to parse the tokens into.</param>
        /// <param name="tokens">The tokens to parse.</param>
        /// <param name="index">A reference to the index of the current token.</param>
        /// <returns>The parse tree.</returns>
        /// <exception cref="ParserException">Thrown if the parser failed to parse the specified tokens.</exception>
        /// <exception cref="UnexpectedTokenException">
        ///     Thrown if an unexpected token was found while parsing the
        ///     specified tokens.
        /// </exception>
        private INode Parse(string rule, Token[] tokens, ref int index)
        {
            var isEof = index >= tokens.Length;
            var token = isEof ? default(Token) : tokens[index];

            if(isEof)
                throw new ParserException("Unexpected end of file.");

            // Find the production rule suitable for the expected rule symbol.
            var productionRule = Table.GetProduction(rule, isEof ? Terminal.EndOfFile : new Terminal(token.Type, token.Value));

            // Verify a production rule was found.
            if (productionRule == null)
                throw new UnexpectedTokenException(rule.ToLowerInvariant(), token);

            // Construct a collection of child nodes for the constructing interior node.
            var nodes = new List<INode>();
            
            foreach (var element in productionRule.Elements)
            {
                isEof = index >= tokens.Length;
                token = isEof ? default(Token) : tokens[index];
                
                switch (element.Type)
                {
                    case ProductionRuleElementType.Terminal:
                        if(isEof)
                            throw new ParserException("Unexpected end of file.");

                        // Verify the current token.
                        if (!element.Terminal.Describes(new Terminal(token.Type, token.Value)))
                            throw new UnexpectedTokenException(element.Terminal, token);

                        nodes.Add(new LeafNode(tokens[index++]));
                        break;
                    case ProductionRuleElementType.NonTerminal:
                        nodes.Add(Parse(element.Value, tokens, ref index));
                        break;
                    default:
                        throw new ParserException("Unexpected production rule element type.");
                }
            }

            return new InteriorNode(rule, nodes);
        }
    }
}