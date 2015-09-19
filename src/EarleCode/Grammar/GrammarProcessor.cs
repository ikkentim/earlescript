// EarleCode
// Copyright 2015 Tim Potze
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
using System.Linq;
using EarleCode.Grammar.RulesElements;
using EarleCode.Parsers;
using EarleCode.Tokens;

namespace EarleCode.Grammar
{
    /// <summary>
    ///     Processes grammar instructions which can be used on tokenizers.
    /// </summary>
    public class GrammarProcessor
    {
        private readonly List<ProductionRule> _rules = new List<ProductionRule>();

        /// <summary>
        ///     Adds a production rule with the specified key and value.
        /// </summary>
        public string this[string key]
        {
            set { Add(key, value); }
        }

        /// <summary>
        ///     Adds a production rule with the specified key and rule.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="rule">The rule.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">Thrown if key or rule is null.</exception>
        /// <exception cref="System.ArgumentException">Invalid key character</exception>
        public ProductionRule Add(string key, string rule)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (rule == null) throw new ArgumentNullException(nameof(rule));

            // Make sure key is upper case.
            key = key.ToUpper();

            // Make sure key consists only of letters, digits or _.
            if (key.Any(c => !char.IsLetterOrDigit(c) && c != '_'))
                throw new ArgumentException("Invalid key character", nameof(key));

            // Tokenize the specified rule.
            var tokenizer = new Tokenizer("production_rules", rule);

            // Convert the tokenizer to a list of tokens.
            // todo should just pass the tokenizer on. this is stupid.
            var tokens = new List<Token>();
            while (tokenizer.MoveNext()) tokens.Add(tokenizer.Current);

            // Add the production rule with the specified key and the compiled ruleset.
            var productionRule = new ProductionRule(key, Compile(tokens.ToArray()));
            _rules.Add(productionRule);

            return productionRule;
        }

        /// <summary>
        ///     Gets the match for the specified tokenizer.
        /// </summary>
        /// <param name="tokenizer">The tokenizer.</param>
        /// <returns>The name of the matching rule.</returns>
        public string GetMatch(ITokenizer tokenizer)
        {
            // Find the first rule which matches the current position of the tokenizer and return its name.
            return _rules.Where(rule => Matches(tokenizer, rule)).Select(rule => rule.Name).FirstOrDefault();
        }

        private static AndProductionRuleElement Compile(Token[] tokens)
        {
            if (tokens == null) throw new ArgumentNullException(nameof(tokens));

            // Create the resulting production rule to which we'll add every element found within the specified tokens.
            var productionRule = new AndProductionRuleElement();

            // Field which holds `true` if a `OPTIONAL` token was found to indicate that the next statement is optional.
            var optional = false;

            // A macro for adding elements to the resulting production rule.
            var add = new Action<IProductionRuleElement>(element =>
            {
                // If `optional` is set, make the element optional.
                element = optional ? new OptionalProductionRuleElement(element) : element;
                optional = false;

                // Add the element to the result.
                productionRule.AddCondition(element);
            });

            // Create a dictionary of token types. The key is the upper case name of the enum and the value the enum
            // value itself. (eg. NumberLiteral: ["NUMBER_LITERAL"] => NumberLiteral)
            var types =
                typeof (TokenType).GetEnumValues()
                    .OfType<TokenType>()
                    .Select(value => new KeyValuePair<string, TokenType>(value.ToUpperString(), value))
                    .ToDictionary(p => p.Key, p => p.Value);

            // Process tokens in the specified tokens array.
            for (var i = 0; i < tokens.Length; i++)
            {
                var token = tokens[i];

                // Detected tokens: 
                // 
                // token                a single token eg. <>+-.
                // `string literal`     a specific string literal
                // OPTIONAL             indicates next token is optional eg. OPTIONAL `test`
                // NUMBER_LITERAL       any number literal eg. 4, 6, 3.14
                // STRING_LITERAL       any string quoted literal eg. "hello, world!"
                // TOKEN                any token eg. <, >, +, -, .
                // IDENTIFIER           any identifier eg. hello, world, test
                // |                    'or' for token types, eg. NUMBER_LITEREAL|STRING_LITERAL, IDENTIFIER|TOKEN          
                // SUB_GRAMMAR          any subsequence name eg. IDENTIFIER = EXPRESSION;

                switch (token.Type)
                {
                    case TokenType.Identifier:
                        // Instruction sequences that start with an identifier:
                        // optional, token types, sub grammar

                        // Check if the current token matches the game of any token type.
                        if (types.ContainsKey(token.Value))
                        {
                            // Create a list of all specified token types, which may be divided by a `|`.
                            var typeLiterals = new List<TokenType>();

                            while (true)
                            {
                                typeLiterals.Add(types[tokens[i].Value]);

                                // If the next token is not a | stop the loop.
                                if (i + 1 >= tokens.Length || tokens[i + 1].Type != TokenType.Token ||
                                    tokens[i + 1].Value != "|")
                                    break;

                                i += 2;

                                // At this point the next token must be an identifier which can be found in the token
                                // types array. If this is not the case, throw an exception.
                                if (i >= tokens.Length || tokens[i].Type != TokenType.Identifier ||
                                    !types.ContainsKey(tokens[i].Value))
                                    throw new ParseException(token, "expected IDENTIFIER, found " + tokens[i].Value);
                            }

                            // Add the element to the result.
                            add(new LiteralProductionRuleElement(typeLiterals.ToArray()));
                        }

                        // If the current token is OPTIONAL, flag the next element optional.
                        else if (token.Value == "OPTIONAL")
                            optional = true;

                        // Any other identifier are sub grammar. Add it as an element to the result.
                        else
                            add(new EmbeddedProductionRuleElement(token.Value));
                        break;
                    case TokenType.Token:
                        // Instruction sequences that start with a token:
                        // `string literal`, token

                        // ` indicates a string literal is coming.
                        if (tokens[i].Value == "`")
                        {
                            // Keep processing tokens until a matching ` is found.
                            while (i + 1 < tokens.Length)
                            {
                                i++;

                                // Any identifier is a string literal which should be added to the result.
                                if (tokens[i].Type == TokenType.Identifier)
                                    add(new LiteralProductionRuleElement(tokens[i].Value, TokenType.Identifier));

                                // The only other accepted token is a ` token. This token will break the search.
                                if (tokens[i].Type == TokenType.Token && tokens[i].Value != "`")
                                    break;

                                throw new ParseException(token, "expected '`', found " + tokens[i].Value);
                            }
                        }

                        // Any other token is a token literal. Add it to the result.
                        else
                            add(new LiteralProductionRuleElement(tokens[i].Value, TokenType.Token));
                        break;

                    // No other tokens are accepted.
                    default:
                        throw new ParseException(token, "expected IDENTIFIER or TOKEN, found " + tokens[i].Value);
                }
            }

            return productionRule;
        }

        /// <summary>
        ///     Matcheses the specified tokenizer againts rules with the specified name.
        /// </summary>
        /// <param name="tokenizer">The tokenizer.</param>
        /// <param name="rule">The rule.</param>
        /// <returns>True if the specified tokenizer matches the specified rule.</returns>
        public bool Matches(ITokenizer tokenizer, string rule)
        {
            // Find any rule which has the specified name and matches the current position of the tokenizer and return its name;
            return _rules.Where(r => r.Name == rule).Any(r => Matches(tokenizer, r));
        }

        private bool Matches(ITokenizer tokenizer, ProductionRule rule)
        {
            // Check whether the rule matches.
            var walker = new TokenWalker(tokenizer);
            var result = rule.Rule.Matches(walker, _rules, 0, rule);

            // Reset the tokenizer.
            walker.DropAllSessions();

            return result;
        }
    }
}