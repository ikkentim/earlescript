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
using System.Linq;
using EarleCode.Compiling.Lexing;
using EarleCode.Compiling.Parsing.Productions;

namespace EarleCode.Compiling.Parsing
{
    /// <summary>
    ///     Represents a LL(1) parser prediction table.
    /// </summary>
    /// <typeparam name="TTokenType">The type enum of the token type.</typeparam>
    public class ParsingTable<TTokenType> where TTokenType : struct, IConvertible
    {
        private readonly Dictionary<string, List<Token<TTokenType>>> _first =
            new Dictionary<string, List<Token<TTokenType>>>();

        private readonly Dictionary<string, List<Token<TTokenType>>> _follow =
            new Dictionary<string, List<Token<TTokenType>>>();

        private readonly IProductionRuleSet<TTokenType> _productionRules;

        private readonly Dictionary<string, Dictionary<Token<TTokenType>, ProductionRule<TTokenType>>> _table =
            new Dictionary<string, Dictionary<Token<TTokenType>, ProductionRule<TTokenType>>>();

        /// <summary>
        ///     Initializes a new instance of the <see cref="ParsingTable{TTokenType}" /> class.
        /// </summary>
        /// <param name="productionRules">The production rules.</param>
        /// <exception cref="ArgumentNullException">productionRules</exception>
        public ParsingTable(IProductionRuleSet<TTokenType> productionRules)
        {
            _productionRules = productionRules ?? throw new ArgumentNullException(nameof(productionRules));

            Construct();
        }

        /// <summary>
        ///     Gets the production rule for the specified <paramref name="symbol" /> when the specified <paramref name="token" />
        ///     is the first next token.
        /// </summary>
        /// <param name="symbol">The symbol.</param>
        /// <param name="token">The next token.</param>
        /// <returns>The production rule.</returns>
        public ProductionRule<TTokenType> GetProduction(string symbol, Token<TTokenType> token)
        {
            var row = _table[symbol];
            
            // Try find the rule for the token.
            if (row.TryGetValue(token, out var cell))
                return cell;

            // Try find the rule for the token, without a specific value.
            token = new Token<TTokenType>(token.Type, null);

            if (row.TryGetValue(token, out cell))
                return cell;

            // Try find the rule an emtpy token.
            token = Token<TTokenType>.Empty;

            row.TryGetValue(token, out cell);

            return cell;
        }

        /// <summary>
        ///     Constructs the sets and tables used by this instance.
        /// </summary>
        private void Construct()
        {
            foreach (var rule in _productionRules.All)
            {
                _first[rule.Name] = new List<Token<TTokenType>>();
                _follow[rule.Name] = new List<Token<TTokenType>>();
                _table[rule.Name] = new Dictionary<Token<TTokenType>, ProductionRule<TTokenType>>();
            }

            ConstructFirstSets();
            ConstructFollowSets();
            ConstructTable();
        }

        /// <summary>
        ///     Constructs the first sets.
        /// </summary>
        private void ConstructFirstSets()
        {
            bool changes;
            do
            {
                changes = false;

                foreach (var rule in _productionRules.All)
                {
                    if (RuleCanBeEmpty(rule))
                    {
                        changes |= AddToFirst(rule.Name, Token<TTokenType>.Empty);
                    }

                    var any = false;
                    foreach (var element in rule.Elements)
                    {
                        switch (element.Type)
                        {
                            case ProductionRuleElementType.Terminal:
                                changes |= AddToFirst(rule.Name, element.Token);
                                any = true;
                                break;
                            case ProductionRuleElementType.NonTerminal:
                                if (!SymbolCanBeEmpty(element.Value))
                                {
                                    changes |= AddToFirstFromFirst(rule.Name, element.Value);
                                    any = true;
                                }
                                break;
                        }
                        if (any)
                            break;
                    }
                    if (!any)
                    {
                        AddToFirst(rule.Name, Token<TTokenType>.Empty);
                    }
                    
                }
            } while (changes);
        }

        /// <summary>
        ///     Constructs the follow sets.
        /// </summary>
        private void ConstructFollowSets()
        {
            bool changes;
            
            AddToFollow(_productionRules.Default, Token<TTokenType>.EndOfFile);

            do
            {
                changes = false;

                foreach (var rule in _productionRules.All)
                    for (var i = 0; i < rule.Elements.Length; i++)
                    {
                        var element = rule.Elements[i];
                        // Find non-terminal elements in all rules.
                        if (element.Type != ProductionRuleElementType.NonTerminal)
                            continue;
                        
                        // If the non-terminal is the last element, add FOLLOW(rule) to FOLLOW(non-terminal)
                        if (i == rule.Elements.Length - 1)
                        {
                            changes |= AddToFollowFromFollow(element.Value, rule.Name);
                            continue;
                        }

                        var nextElement = rule.Elements[i + 1];

                        switch (nextElement.Type)
                        {
                            case ProductionRuleElementType.Terminal:
                                changes |= AddToFollow(element.Value, nextElement.Token);
                                break;
                            case ProductionRuleElementType.NonTerminal:
                                changes |= AddToFollowFromFirst(element.Value, nextElement.Value);
                                break;
                            case ProductionRuleElementType.TerminalEmpty:
                                throw new NotImplementedException();
                        }

                        var remainingElementsCanBeEmpty = rule.Elements.Skip(i + 1).All(ElementCanBeEmpty);
                        if (remainingElementsCanBeEmpty)
                            changes |= AddToFollowFromFollow(element.Value, rule.Name);
                    }
            } while (changes);
        }

        /// <summary>
        ///     Constructs the prediction table.
        /// </summary>
        private void ConstructTable()
        {
            foreach (var rule in _productionRules.All)
            {
                var firstElement = rule.Elements[0];

                switch (firstElement.Type)
                {
                    case ProductionRuleElementType.Terminal:
                        AddToTable(rule.Name, firstElement.Token, rule);
                        break;
                    case ProductionRuleElementType.NonTerminal:
                        foreach (var t in _first[firstElement.Value].Where(t => !t.IsEmpty))
                            AddToTable(rule.Name, t, rule);

                        if (_first[firstElement.Value].Any(t => t.IsEmpty))
                            foreach (var t in _follow[rule.Name])
                                AddToTable(rule.Name, t, rule);
                        break;
                    case ProductionRuleElementType.TerminalEmpty:
                        foreach (var t in _follow[rule.Name])
                            AddToTable(rule.Name, t, rule);
                        break;
                }
            }
        }
        
        public void DebugPrint()
        {
            Console.WriteLine();
            Console.WriteLine("Rules:");
            foreach (var rule in _productionRules.All)
            {
                Console.WriteLine(rule);
            }

            Console.WriteLine();
            Console.WriteLine("First sets:");
            foreach (var first in _first)
            {
                Console.WriteLine(first.Key + ":");

                foreach (var n in first.Value)
                {
                    Console.WriteLine("  " + n);
                }
            }

            Console.WriteLine();
            Console.WriteLine("Follow sets:");
            foreach (var follow in _follow)
            {
                Console.WriteLine(follow.Key + ":");

                foreach (var n in follow.Value)
                {
                    Console.WriteLine("  " + n);
                }
            }

            Console.WriteLine();
            Console.WriteLine("Table:");
            foreach (var row in _table)
            {
                Console.WriteLine("row " + row.Key + ":");

                foreach (var col in row.Value)
                {
                    Console.WriteLine("  column " + col.Key + " -> " + col.Value);
                }
            }

        }

        /// <summary>
        ///     Returns a value indicating whether the speified element can be empty.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns>A value indicating whether the speified element can be empty.</returns>
        private bool ElementCanBeEmpty(ProductionRuleElement<TTokenType> element)
        {
            return element.Type == ProductionRuleElementType.TerminalEmpty ||
                   element.Type == ProductionRuleElementType.NonTerminal && SymbolCanBeEmpty(element.Value);
        }

        /// <summary>
        ///     Rules a value indicating whether the specified rule can be empty.
        /// </summary>
        /// <param name="rule">The rule.</param>
        /// <returns></returns>
        private static bool RuleCanBeEmpty(ProductionRule<TTokenType> rule)
        {
            return rule.Elements.Length == 0 || rule.Elements.Length == 1 && rule.Elements[0].Token.IsEmpty;
        }

        /// <summary>
        ///     Returns a value indicating whether the specified symbol the can be empty.
        /// </summary>
        /// <param name="symbol">The symbol.</param>
        /// <returns>A value indicating whether the specified symbols the can be empty.</returns>
        private bool SymbolCanBeEmpty(string symbol)
        {
            return _productionRules.Get(symbol).Any(RuleCanBeEmpty);
        }

        /// <summary>
        ///     Adds the specified value to the table.
        /// </summary>
        /// <param name="row">The row.</param>
        /// <param name="column">The column.</param>
        /// <param name="value">The value to add..</param>
        /// <exception cref="GrammarException">Thrown if the grammar is ambiguous.</exception>
        private void AddToTable(string row, Token<TTokenType> column, ProductionRule<TTokenType> value)
        {
            if (_table[row].ContainsKey(column))
                throw new GrammarException($"{row} is ambiguous (near {column}).");

            _table[row][column] = value;
        }

        /// <summary>
        ///     Adds the tokens from a first set to a first set.
        /// </summary>
        /// <param name="symbol">The symbol of the set.</param>
        /// <param name="fromSymbol">The symbol of the set to add from.</param>
        /// <returns>true if the value was added; otherwise false.</returns>
        private bool AddToFirstFromFirst(string symbol, string fromSymbol)
        {
            return _first[fromSymbol]
                .Aggregate(false, (current, token) => current | AddToFirst(symbol, token));
        }

        /// <summary>
        ///     Adds the tokens from a first set to a follow set, except for emtpy tokens.
        /// </summary>
        /// <param name="symbol">The symbol of the set.</param>
        /// <param name="fromSymbol">The symbol of the set to add from.</param>
        /// <returns>true if the value was added; otherwise false.</returns>
        private bool AddToFollowFromFirst(string symbol, string fromSymbol)
        {
            return _first[fromSymbol]
                .Where(token => !token.IsEmpty)
                .Aggregate(false, (current, token) => current | AddToFollow(symbol, token));
        }

        /// <summary>
        ///     Adds the tokens from a follow set to a follow set.
        /// </summary>
        /// <param name="symbol">The symbol of the set.</param>
        /// <param name="fromSymbol">The symbol of the set to add from.</param>
        /// <returns>true if the value was added; otherwise false.</returns>
        private bool AddToFollowFromFollow(string symbol, string fromSymbol)
        {
            return _follow[fromSymbol]
                .Aggregate(false, (current, token) => current | AddToFollow(symbol, token));
        }

        /// <summary>
        ///     Adds a token to a first set.
        /// </summary>
        /// <param name="symbol">The symbol of the set.</param>
        /// <param name="token">The token.</param>
        /// <returns>true if the value was added; otherwise false.</returns>
        private bool AddToFirst(string symbol, Token<TTokenType> token)
        {
            var set = _first[symbol];
            if (set.Contains(token)) return false;
            set.Add(token);
            return true;
        }

        /// <summary>
        ///     Adds a token to a follow set.
        /// </summary>
        /// <param name="symbol">The symbol of the set.</param>
        /// <param name="token">The tokent.</param>
        /// <returns>true if the value was added; otherwise false.</returns>
        private bool AddToFollow(string symbol, Token<TTokenType> token)
        {
            var set = _follow[symbol];
            if (set.Contains(token)) return false;
            set.Add(token);
            return true;
        }
    }
}