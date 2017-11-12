﻿// EarleCode
// Copyright 2017 Tim Potze
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
using EarleCode.Compiling.Lexing;
using EarleCode.Compiling.Parsing.AST;
using EarleCode.Compiling.Parsing.Productions;

namespace EarleCode.Compiling.Parsing
{
    /// <summary>
    ///     Represents a basic LL(1) parser.
    /// </summary>
    /// <typeparam name="TTokenType">Type type enum of the token type.</typeparam>
    public class Parser<TTokenType> : IParser<TTokenType> where TTokenType : struct, IConvertible
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="Parser{TTokenType}" /> class.
        /// </summary>
        /// <param name="productionRules">The production rules.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="productionRules" /> is null.</exception>
        public Parser(IProductionRuleSet<TTokenType> productionRules)
        {
            ProductionRules = productionRules ?? throw new ArgumentNullException(nameof(productionRules));
            Table = new ParsingTable<TTokenType>(productionRules);
        }

        /// <summary>
        ///     Gets the production rules used by this parser.
        /// </summary>
        public IProductionRuleSet<TTokenType> ProductionRules { get; }

        /// <summary>
        ///     Gets the parsing table used by this parser.
        /// </summary>
        public ParsingTable<TTokenType> Table { get; }

        #region Implementation of IParser<TokenType>

        /// <summary>
        ///     Parses the specified tokens into an abstract syntax tree.
        /// </summary>
        /// <param name="tokens">The tokens to parse.</param>
        /// <returns>The parsed syntax tree.</returns>
        public IAbstractSyntaxTreeNode Parse(Token<TTokenType>[] tokens)
        {
            var index = 0;
            return Parse(ProductionRules.Default, tokens, ref index);
        }

        #endregion

        /// <summary>
        /// Gets the token at the specified <paramref name="index"/>. If the <paramref name="index"/> is past the upper bounds of the <paramref name="tokens"/> array, an empty token is returned.
        /// </summary>
        /// <param name="tokens">The tokens to get the token from.</param>
        /// <param name="index">The index of the token.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if index is out of bounds.</exception>
        /// <returns>The token.</returns>
        private Token<TTokenType> GetToken(Token<TTokenType>[] tokens, int index)
        {
            if (index < 0) throw new ArgumentOutOfRangeException(nameof(index));
            return index >= tokens.Length ? Token<TTokenType>.EndOfFile : tokens[index];
        }

        /// <summary>
        ///     Parses the specified tokens with the specified rule.
        /// </summary>
        /// <param name="rule">The symbol of the rule to parse the tokens into.</param>
        /// <param name="tokens">The tokens to parse.</param>
        /// <param name="index">A reference to the index of the current token.</param>
        /// <returns>The parsed syntax tree.</returns>
        /// <exception cref="ParserException">Thrown if the parser failed to parse the specified tokens.</exception>
        /// <exception cref="UnexpectedTokenException{TTokenType}">
        ///     Thrown if an unexpected token was found while parsing the
        ///     specified tokens.
        /// </exception>
        private IAbstractSyntaxTreeNode Parse(string rule, Token<TTokenType>[] tokens, ref int index)
        {
            var token = GetToken(tokens, index);

            // Find the production rule suitable for the expected rule symbol.
            var productionRule = Table.GetProduction(rule, token);

            // Verify a production rule was found.
            if (productionRule == null)
                throw new UnexpectedTokenException<TTokenType>(rule.ToLowerInvariant(), token);

            // Construct a collection of child nodes for the constructing interior node.
            var nodes = new List<IAbstractSyntaxTreeNode>();
            foreach (var element in productionRule.Elements)
            {
                token = GetToken(tokens, index);
                switch (element.Type)
                {
                    case ProductionRuleElementType.Terminal:
                        // Verify the current token.
                        if (!element.Token.Describes(token))
                            throw new UnexpectedTokenException<TTokenType>(element.Token, token);

                        nodes.Add(new AbstractSyntaxTreeLeafNode<TTokenType>(tokens[index++]));
                        break;
                    case ProductionRuleElementType.NonTerminal:
                        nodes.Add(Parse(element.Value, tokens, ref index));
                        break;
                    case ProductionRuleElementType.TerminalEmpty:
                        nodes.Add(new AbstractSyntaxTreeLeafNode<TTokenType>(Token<TTokenType>.Empty));
                        break;
                    default:
                        throw new ParserException("Unexpected production rule element type.");
                }
            }

            return new AbstractSyntaxTreeInteriorNode(rule, nodes);
        }
    }
}