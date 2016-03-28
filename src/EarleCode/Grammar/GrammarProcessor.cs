// EarleCode
// Copyright 2016 Tim Potze
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
using EarleCode.Lexing;
using EarleCode.Parsers;

namespace EarleCode.Grammar
{
    /// <summary>
    ///     Processes grammar instructions which can be used on tokenizers.
    /// </summary>
    public class GrammarProcessor
    {
        private readonly List<GrammarRule> _rules = new List<GrammarRule>();

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
        public GrammarRule Add(string key, string rule)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (rule == null) throw new ArgumentNullException(nameof(rule));

            // Make sure key is upper case.
            key = key.ToUpper();

            // Make sure key consists only of letters, digits or _.
            if (key.Any(c => !char.IsLetterOrDigit(c) && c != '_'))
                throw new ArgumentException("Invalid key character", nameof(key));

            // Tokenize the specified rule.
            var tokenizer = new Lexer("production_rules", rule);

            // Convert the lexer to a list of tokens.
            // todo should just pass the lexer on. this is stupid.
            var tokens = new List<Token>();
            while (tokenizer.MoveNext()) tokens.Add(tokenizer.Current);

            // Add the production rule with the specified key and the compiled ruleset.
            var productionRule = new GrammarRule(key, Compile(tokens.ToArray()));
            _rules.Add(productionRule);

            return productionRule;
        }

        /// <summary>
        ///     Gets the match for the specified lexer.
        /// </summary>
        /// <param name="lexer">The lexer.</param>
        /// <returns>The name of the matching rule.</returns>
        public string GetMatch(ILexer lexer)
        {
            // Find the first rule which matches the current position of the lexer and return its name.
            return _rules.Where(rule => Matches(lexer, rule)).Select(rule => rule.Name).FirstOrDefault();
        }

        private static AndGrammarRuleElement Compile(Token[] tokens)
        {
            if (tokens == null) throw new ArgumentNullException(nameof(tokens));

            // Create the resulting production rule to which we'll add every element found within the specified tokens.
            var productionRule = new AndGrammarRuleElement();

            // Field which holds `true` if a `OPTIONAL` token was found to indicate that the next statement is optional.
            var optional = false;

            // A macro for adding elements to the resulting production rule.
            var add = new Action<IGrammarRuleElement>(element =>
            {
                // If `optional` is set, make the element optional.
                element = optional ? new OptionalGrammarRuleElement(element) : element;
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
                            add(new LiteralGrammarRuleElement(typeLiterals.ToArray()));
                        }

                        // If the current token is OPTIONAL, flag the next element optional.
                        else if (token.Value == "OPTIONAL")
                            optional = true;

                        // Any other identifier are sub grammar. Add it as an element to the result.
                        else
                            add(new EmbeddedGrammarRuleElement(token.Value));
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
                                    add(new LiteralGrammarRuleElement(tokens[i].Value, TokenType.Identifier));

                                // The only other accepted token is a ` token. This token will break the search.
                                else if (tokens[i].Type == TokenType.Token && tokens[i].Value == "`")
                                    break;
                                else
                                    throw new ParseException(token, "expected '`', found " + tokens[i].Value);
                            }
                        }

                        // Any other token is a token literal. Add it to the result.
                        else
                            add(new LiteralGrammarRuleElement(tokens[i].Value, TokenType.Token));
                        break;

                    // No other tokens are accepted.
                    default:
                        throw new ParseException(token, "expected IDENTIFIER or TOKEN, found " + tokens[i].Value);
                }
            }

            return productionRule;
        }

        /// <summary>
        ///     Matcheses the specified lexer againts rules with the specified name.
        /// </summary>
        /// <param name="lexer">The lexer.</param>
        /// <param name="rule">The rule.</param>
        /// <returns>True if the specified lexer matches the specified rule.</returns>
        public bool Matches(ILexer lexer, string rule)
        {
            // Find any rule which has the specified name and matches the current position of the lexer and return its name;
            return _rules.Where(r => r.Name == rule).Any(r => Matches(lexer, r));
        }

        private bool Matches(ILexer lexer, GrammarRule rule)
        {
            // Check whether the rule matches.
            var walker = new TokenWalker(lexer);
            var result = rule.Rule.Matches(walker, _rules, 0, rule);

            // Reset the lexer.
            walker.DropAllSessions();

            return result;
        }
    }
}