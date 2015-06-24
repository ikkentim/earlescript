﻿// Earle
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
using Earle.Grammars.ProductionRuleElements;
using Earle.Tokens;

namespace Earle.Grammars
{
    public class Grammar
    {
        private readonly List<ProductionRule> _rules = new List<ProductionRule>();

        public ProductionRule AddProductionRule(string key, string rule)
        {
            if (key == null) throw new ArgumentNullException("key");
            if (rule == null) throw new ArgumentNullException("rule");

            key = key.ToUpper();

            if (key.Any(c => !char.IsLetterOrDigit(c) && c != '_'))
                throw new ArgumentException("Invalid key character", "key");

            var tokenizer = new Tokenizer(rule);

            var tokens = new List<Token>();
            do tokens.Add(tokenizer.Current); while (tokenizer.MoveNext());

            var productionRule = new ProductionRule(key, Compile(tokens.ToArray()));
            _rules.Add(productionRule);

            return productionRule;
        }

        public string GetMatch(Tokenizer tokenizer)
        {
            return _rules.Where(rule => Matches(tokenizer, rule)).Select(rule => rule.Name).FirstOrDefault();
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

            var types = typeof (TokenType).GetEnumNames().Select(value =>
            {
                return
                    new KeyValuePair<string, TokenType>(
                        string.Concat(
                            value.Select((x, i) => i > 0 && char.IsUpper(x) ? "_" + x.ToString() : x.ToString()))
                            .ToUpper(),
                        (TokenType) Enum.Parse(typeof (TokenType), value));
            }).ToDictionary(p => p.Key, p => p.Value);

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
                                    throw new Exception("Unexpected token");
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
                                        throw new Exception();

                                    break;
                                }

                                add(new LiteralProductionRuleElement(tokens[i].Value, TokenType.Identifier));
                            }
                        }
                        else
                        {
                            add(new LiteralProductionRuleElement(tokens[i].Value, TokenType.Token));
                        }
                        break;
                    default:
                        throw new Exception("Unexpected token type");
                }
            }

            return productionRule;
        }

        private bool Matches(Tokenizer tokenizer, ProductionRule rule)
        {
            var walker = new TokenWalker(tokenizer);
            var result = rule.Rule.Matches(walker, _rules);

            walker.DropAllSessions();

            return result;
        }
    }
}