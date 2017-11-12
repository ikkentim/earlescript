// EarleCode
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
using System.Globalization;
using System.Linq;
using EarleCode.Compiling.Lexing;
using EarleCode.Utilities;

namespace EarleCode.Compiling.Parsing.Productions
{
    /// <summary>
    ///     Represents a production rules set which is constructed based on the specified
    ///     <typeparamref name="TProductionRulesEnum" /> and the <see cref="RuleAttribute" /> attributes attached to its
    ///     values.
    /// </summary>
    /// <typeparam name="TProductionRulesEnum">The type of the product rule enum.</typeparam>
    /// <typeparam name="TTokenType">The type of the token type.</typeparam>
    /// <seealso cref="IProductionRuleSet{TTokenType}" />
    public class EnumProductionRuleSet<TProductionRulesEnum, TTokenType> : IProductionRuleSet<TTokenType>
        where TProductionRulesEnum : struct, IConvertible
        where TTokenType : struct, IConvertible
    {
        private readonly ProductionRuleSet<TTokenType> _set;

        /// <summary>
        ///     Initializes a new instance of the <see cref="EnumProductionRuleSet{TProductionRulesEnum, TTokenType}" /> class.
        /// </summary>
        /// <exception cref="GrammarException">Thrown when an unknown token was found in the grammar.</exception>
        public EnumProductionRuleSet()
        {
            _set = new ProductionRuleSet<TTokenType>();
            Construct();
        }

        /// <summary>
        ///     Gets a production rule element for the specified token.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="ctx">The context.</param>
        /// <returns>The element for the token</returns>
        /// <exception cref="EarleCode.Compiling.Parsing.Productions.GrammarException">Thrown if the token is invalid.</exception>
        private ProductionRuleElement<TTokenType> GetElement(Token<GrammarTokenType> token, ConstructionContext ctx)
        {
            switch (token.Type)
            {
                case GrammarTokenType.ProductionSymbol:
                    // If the token is a production symbol, find the associated terminal or non-terminal.
                    if (ctx.Tokens.TryGetValue(token.Value, out var tokenType))
                        return new ProductionRuleElement<TTokenType>(tokenType, null);
                    else if (ctx.Symbols.ContainsKey(token.Value))
                        return new ProductionRuleElement<TTokenType>(ProductionRuleElementType.NonTerminal,
                            token.Value);
                    else
                        throw new GrammarException($"Unknown token {token:B} in grammar");
                case GrammarTokenType.Symbol:
                    // If the token is a symbol, add it as a terminal.
                    return new ProductionRuleElement<TTokenType>(default(TTokenType), token.Value);
                case GrammarTokenType.Keyword:
                    // If the token is a keyword, validate and add it.
                    var tokenValue = token.Value.Substring(1, token.Value.Length - 2)
                        .Replace("\\`", "`")
                        .Replace("\\\\", "\\");

                    if (tokenValue.Length <= 0)
                        throw new GrammarException("Keywords in grammar cannot be empty.");

                    if (tokenValue.Any(char.IsWhiteSpace))
                        throw new GrammarException("Keywords in grammar cannot contain white space characters.");

                    return new ProductionRuleElement<TTokenType>(ctx.Keyword, tokenValue);
                default:
                    throw new GrammarException("Unknown token " + token + " in grammar");
            }
        }

        /// <summary>
        ///     Constructs this instance.
        /// </summary>
        /// <exception cref="GrammarException">Thrown when an unknown token was found in the grammar.</exception>
        private void Construct()
        {
            // Construct dictionaries of known token types and production rule symbols
            var ctx = new ConstructionContext
            {
                Tokens = new Dictionary<string, TTokenType>(),
                Symbols = new Dictionary<string, TProductionRulesEnum>()
            };
            foreach (var value in typeof(TProductionRulesEnum).GetEnumValues().Cast<TProductionRulesEnum>())
                ctx.Symbols[value.ToString(CultureInfo.InvariantCulture)] = value;

            foreach (var value in typeof(TTokenType).GetEnumValues().Cast<TTokenType>())
                ctx.Tokens[value.ToString(CultureInfo.InvariantCulture)] = value;

            ctx.Keyword = (TTokenType?) ctx.Tokens.Values.Cast<object>()
                              .FirstOrDefault(n => ((Enum) n).GetCustomAttribute<KeywordAttribute>() !=
                                                   null) ?? default(TTokenType);
            
            foreach (var value in typeof(TProductionRulesEnum).GetEnumValues().Cast<Enum>())
            {
                // Find the rule strings attached to the value.
                var rules = value.GetCustomAttributes<RuleAttribute>()
                    .SelectMany(a => a == null || a.Rules.Length == 0 ? new[] {""} : a.Rules)
                    .Distinct()
                    .ToArray();

                // If no rule has been attached add an empty rule.
                if (rules.Length == 0)
                    rules = new[] {""};

                // Construct production rule instances for each rule value.
                foreach (var rule in rules)
                {
                    var lexer = new Lexer<GrammarTokenType>();

                    var elements = lexer.Tokenize(rule, "rule").Select(token => GetElement(token, ctx)).ToArray();

                    if (elements.Length == 0)
                        elements = new[]
                        {
                            new ProductionRuleElement<TTokenType>()
                        };

                    _set.Add(value.ToString(), new ProductionRule<TTokenType>(value.ToString(), elements.ToArray()));
                }


                if (_set.Default == null)
                    _set.Default = value.ToString();
            }
        }

        private struct ConstructionContext
        {
            public Dictionary<string, TTokenType> Tokens;
            public Dictionary<string, TProductionRulesEnum> Symbols;
            public TTokenType Keyword;
        }

        private enum GrammarTokenType : byte
        {
            Symbol,

            [TokenRegex(@"\G[a-zA-Z_][a-zA-Z0-9_]*")]
            ProductionSymbol,

            [TokenRegex(@"\G([`])((?:\\\1|.)*?)\1")]
            Keyword
        }

        #region Implementation of IProductionRuleSet

        /// <summary>
        ///     Gets the default production symbol.
        /// </summary>
        public string Default => _set.Default;

        /// <summary>
        ///     Gets a collection of all available production rules.
        /// </summary>
        public IEnumerable<ProductionRule<TTokenType>> All => _set.All;

        /// <summary>
        ///     Gets a collection of all available production rules which can be represented by the specified
        ///     <paramref name="symbol" />.
        /// </summary>
        /// <param name="symbol">The symbol of the production rules.</param>
        /// <returns>
        ///     A collection of all available production rules which can be represented by the specified symbol.
        /// </returns>
        public IEnumerable<ProductionRule<TTokenType>> Get(string symbol)
        {
            return _set.Get(symbol);
        }

        #endregion
    }
}