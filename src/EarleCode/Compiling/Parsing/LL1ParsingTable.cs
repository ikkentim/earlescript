using System;
using System.Collections.Generic;
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

        private readonly Dictionary<string, Dictionary<Terminal, ProductionRule>> _table =
            new Dictionary<string, Dictionary<Terminal, ProductionRule>>();

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
        public ProductionRule GetProduction(string symbol, Terminal token)
        {
            var row = _table[symbol];
            
            // Try find the rule for the token.
            if (row.TryGetValue(new Terminal(token.TokenType, token.Value), out var cell))
                return cell;

            // Try find the rule for the token, without a specific value.
            row.TryGetValue(new Terminal(token.TokenType, null), out cell);
            
            return cell;
        }

        /// <summary>
        ///     Constructs the sets and tables used by this instance.
        /// </summary>
        private void Construct()
        {
            foreach (var rule in _grammar.All)
            {
                _table[rule.Name] = new Dictionary<Terminal, ProductionRule>();
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
                var containsEpsilon = true;

                if (rule.Elements.Length == 0)
                {  
                    foreach (var t in _follow[rule.Name])
                        AddToTable(rule.Name, t, rule);
                }
                
                foreach (var element in rule.Elements)
                {
                    containsEpsilon = false;
                    
                    switch (element.Type)
                    {
                        case ProductionRuleElementType.Terminal:
                            AddToTable(rule.Name, element.Terminal, rule);
                            break;
                        case ProductionRuleElementType.NonTerminal:
                            foreach (var t in _first[element.Value])
                            {
                                if (t.Type == TerminalType.Epsilon)
                                {
                                    containsEpsilon = true;
                                }
                                else
                                {
                                    AddToTable(rule.Name, t, rule);
                                }
                            }


                            break;
                    }

                    if (!containsEpsilon)
                        break;
                }

                if (containsEpsilon)
                    foreach (var t in _follow[rule.Name])
                        AddToTable(rule.Name, t, rule);
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
        /// <param name="value">The value to add.</param>
        /// <exception cref="GrammarException">Thrown if the grammar is ambiguous.</exception>
        private void AddToTable(string row, Terminal column, ProductionRule value)
        {
            if (_table[row].TryGetValue(column, out var ex) && value != ex)
                throw new GrammarException($"{row} is ambiguous (near {column}).");

            _table[row][column] = value;
        }
    }
}