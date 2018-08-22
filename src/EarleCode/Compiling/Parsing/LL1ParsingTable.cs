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
using EarleCode.Compiling.Parsing.Grammars;
using EarleCode.Compiling.Parsing.Grammars.Productions;

namespace EarleCode.Compiling.Parsing
{
    /// <summary>
    ///     Represents a LL(1) parser prediction table.
    /// </summary>
    public class LL1ParsingTable
    {
        private FirstSet _first;
        private FollowSet _follow;

        private readonly IGrammar _grammar;

        private readonly Dictionary<string, Dictionary<Token, ProductionRule>> _table =
            new Dictionary<string, Dictionary<Token, ProductionRule>>();

        /// <summary>
        ///     Initializes a new instance of the <see cref="LL1ParsingTable" /> class.
        /// </summary>
        /// <param name="grammar">The production rules.</param>
        /// <exception cref="ArgumentNullException">productionRules</exception>
        public LL1ParsingTable(IGrammar grammar)
        {
            _grammar = grammar ?? throw new ArgumentNullException(nameof(grammar));

            Construct();
        }

        /// <summary>
        ///     Gets the production rule for the specified <paramref name="symbol" /> when the specified <paramref name="token" />
        ///     is the first next token.
        /// </summary>
        /// <param name="symbol">The symbol.</param>
        /// <param name="token">The next token.</param>
        /// <returns>The production rule.</returns>
        public ProductionRule GetProduction(string symbol, Token token)
        {
            var row = _table[symbol];
            
            // Try find the rule for the token.
            if (row.TryGetValue(token, out var cell))
                return cell;

            // Try find the rule for the token, without a specific value.
            token = new Token(token.Type, null);

            if (row.TryGetValue(token, out cell))
                return cell;

            // Try find the rule an emtpy token.
            token = Token.Empty;

            row.TryGetValue(token, out cell);

            return cell;
        }

        /// <summary>
        ///     Constructs the sets and tables used by this instance.
        /// </summary>
        private void Construct()
        {
            foreach (var rule in _grammar.All)
            {
                _table[rule.Name] = new Dictionary<Token, ProductionRule>();
            }

            _first = new FirstSet(_grammar);
            _follow = new FollowSet(_grammar, _first);
            ConstructTable();
        }

        /// <summary>
        ///     Constructs the prediction table.
        /// </summary>
        private void ConstructTable()
        {
            foreach (var rule in _grammar.All)
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
            foreach (var rule in _grammar.All)
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
        ///     Adds the specified value to the table.
        /// </summary>
        /// <param name="row">The row.</param>
        /// <param name="column">The column.</param>
        /// <param name="value">The value to add..</param>
        /// <exception cref="GrammarException">Thrown if the grammar is ambiguous.</exception>
        private void AddToTable(string row, Token column, ProductionRule value)
        {
            if (_table[row].ContainsKey(column))
                throw new GrammarException($"{row} is ambiguous (near {column}).");

            _table[row][column] = value;
        }
    }
}