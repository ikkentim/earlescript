// Earle
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
using System.Data;
using System.Linq;
using Earle.Grammars.ProductionRuleElements;
using Earle.Tokens;

namespace Earle.Grammars
{
    public class Grammar
    {
        private readonly List<ProductionRule> _rules = new List<ProductionRule>();

        public ProductionRule Add(string key, string rule)
        {
            if (key == null) throw new ArgumentNullException("key");
            if (rule == null) throw new ArgumentNullException("rule");

            key = key.ToUpper();

            if (key.Any(c => !char.IsLetterOrDigit(c) && c != '_'))
                throw new ArgumentException("Invalid key character", "key");

            var tokenizer = new Tokenizer("production_rules", rule);

            var tokens = new List<Token>();

            while (tokenizer.MoveNext()) tokens.Add(tokenizer.Current);

            var productionRule = new ProductionRule(key, Compile(tokens.ToArray()));
            _rules.Add(productionRule);

            return productionRule;
        }

        public string GetMatch(Tokenizer tokenizer)
        {
            return _rules.Where(rule => Matches(tokenizer, rule)).Select(rule => rule.Name).FirstOrDefault();
        }

        public string GetMatch(Tokenizer tokenizer, out int tokens)
        {
            foreach (var rule in _rules)
                if (Matches(tokenizer, rule, out tokens))
                   return rule.Name;
            
            tokens = 0;
            return null;
        }

        private static AndProductionRuleElement Compile(Token[] tokens)
        {
            if (tokens == null) throw new ArgumentNullException("tokens");

            var productionRule = new AndProductionRuleElement();

            var optional = false;

            var add = new Action<IProductionRuleElement>(element =>
            {
                element = optional ? new OptionalProductionRuleElement(element) : element;
                optional = false;

                productionRule.AddCondition(element);
            });

            var types =
                typeof (TokenType).GetEnumValues()
                    .OfType<TokenType>()
                    .Select(value => new KeyValuePair<string, TokenType>(value.ToUpperString(), value))
                    .ToDictionary(p => p.Key, p => p.Value);

            for (var i = 0; i < tokens.Length; i++)
            {
                var token = tokens[i];

                switch (token.Type)
                {
                    case TokenType.Identifier:
                        if (types.ContainsKey(token.Value))
                        {
                            var typeLiterals = new List<TokenType>();

                            // Take all chained literals
                            while (true)
                            {
                                typeLiterals.Add(types[tokens[i].Value]);

                                if (i + 1 >= tokens.Length || tokens[i + 1].Type != TokenType.Token ||
                                    tokens[i + 1].Value != "|")
                                    break;

                                i += 2;

                                if (i >= tokens.Length || tokens[i].Type != TokenType.Identifier ||
                                    !types.ContainsKey(tokens[i].Value))
                                    throw new GrammarException(token, "expected IDENTIFIER, found " + tokens[i].Value);
                            }

                            add(new LiteralProductionRuleElement(typeLiterals.ToArray()));
                        }
                        else if (token.Value == "OPTIONAL")
                            optional = true;
                        else
                            add(new EmbeddedProductionRuleElement(token.Value));
                        break;
                    case TokenType.Token:
                        if (tokens[i].Value == "`")
                        {
                            while (i + 1 < tokens.Length)
                            {
                                i++;
                                if (tokens[i].Type != TokenType.Identifier)
                                {
                                    if (tokens[i].Type != TokenType.Token || tokens[i].Value != "`")
                                        throw new GrammarException(token, "expected '`', found " + tokens[i].Value);

                                    break;
                                }

                                add(new LiteralProductionRuleElement(tokens[i].Value, TokenType.Identifier));
                            }
                        }
                        else
                            add(new LiteralProductionRuleElement(tokens[i].Value, TokenType.Token));
                        break;
                    default:
                        throw new GrammarException(token, "expected IDENTIFIER or TOKEN, found " + tokens[i].Value);
                }
            }

            return productionRule;
        }

        public bool Matches(Tokenizer tokenizer, string rule)
        {
            return _rules.Where(r => r.Name == rule).Any(r => Matches(tokenizer, r));
        }

        private bool Matches(Tokenizer tokenizer, ProductionRule rule)
        {
            var walker = new TokenWalker(tokenizer);
            var result = rule.Rule.Matches(walker, _rules, 0, rule);

            walker.DropAllSessions();

            return result;
        }

        private bool Matches(Tokenizer tokenizer, ProductionRule rule, out int tokens)
        {
            var walker = new TokenWalker(tokenizer);
            var result = rule.Rule.Matches(walker, _rules, 0, rule);
            tokens = walker.SessionTokenCount;
            walker.DropAllSessions();

            return result;
        }
    }
}