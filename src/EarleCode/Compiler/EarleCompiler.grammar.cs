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

using System.Linq;
using EarleCode.Compiler.Grammar;
using EarleCode.Runtime;
using EarleCode.Runtime.Operators;

namespace EarleCode.Compiler
{
    internal partial class EarleCompiler
    {
        private GrammarProcessor _fileGrammarProcessor;

        /// <summary>
        ///     Gets the grammar associated with this compiler.
        /// </summary>
        public GrammarProcessor SyntaxGrammarProcessor { get; private set; }

        private void InitializeGrammarProcessor()
        {
            _fileGrammarProcessor = new GrammarProcessor();
            _fileGrammarProcessor.AddRule("FUNCTION_DECLARATION", true, "IDENTIFIER ( OPTIONAL IDENTIFIER_LIST )");
            _fileGrammarProcessor.AddRule("INCLUDE", true, "# `include` PATH ;");
            _fileGrammarProcessor.AddRule("IDENTIFIER_LIST", false, "IDENTIFIER_LIST , IDENTIFIER_LIST");
            _fileGrammarProcessor.AddRule("IDENTIFIER_LIST", false, "IDENTIFIER");
            _fileGrammarProcessor.AddRule("PATH", false, "OPTIONAL \\ IDENTIFIER");
            _fileGrammarProcessor.AddRule("PATH", false, "PATH \\ IDENTIFIER");
          
            // Statements
            SyntaxGrammarProcessor = new Grammar.GrammarProcessor();
            SyntaxGrammarProcessor.AddRule("STATEMENT_IF", true, "`if` ( EXPRESSION )");
            SyntaxGrammarProcessor.AddRule("STATEMENT_DO", true, "`do`");
            SyntaxGrammarProcessor.AddRule("STATEMENT_WHILE", true, "`while` ( EXPRESSION )");
            SyntaxGrammarProcessor.AddRule("STATEMENT_FOR", true, "`for` ( OPTIONAL ASSIGNMENT ; OPTIONAL EXPRESSION ; OPTIONAL ASSIGNMENT )");
            SyntaxGrammarProcessor.AddRule("STATEMENT_SWITCH", true, "`switch` ( EXPRESSION )");
            SyntaxGrammarProcessor.AddRule("STATEMENT_RETURN", true, "`return` OPTIONAL EXPRESSION ;");
            SyntaxGrammarProcessor.AddRule("STATEMENT_BREAK", true, "`break` ;");
            SyntaxGrammarProcessor.AddRule("STATEMENT_CONTINUE", true, "`continue` ;");
            SyntaxGrammarProcessor.AddRule("STATEMENT_WAIT", true, "`wait` EXPRESSION ;");
            SyntaxGrammarProcessor.AddRule("ASSIGNMENT", true, "VARIABLE = EXPRESSION");
            SyntaxGrammarProcessor.AddRule("ASSIGNMENT", true, "VARIABLE OPERATOR_ASSIGNMENT = EXPRESSION");
            SyntaxGrammarProcessor.AddRule("ASSIGNMENT", true, "VARIABLE OPERATOR_MOD_ASSIGNMENT");
            SyntaxGrammarProcessor.AddRule("ASSIGNMENT", true, "OPERATOR_MOD_ASSIGNMENT VARIABLE");
            SyntaxGrammarProcessor.AddRule("FUNCTION_CALL", true, "OPTIONAL VARIABLE OPTIONAL `thread` FUNCTION_CALL_PART");
            SyntaxGrammarProcessor.AddRule("FUNCTION_CALL_PART", false, "FUNCTION_IDENTIFIER ( OPTIONAL EXPRESSION_LIST )");

            // Labels
            SyntaxGrammarProcessor.AddRule("LABEL_CASE", false, "`case` NUMBER_LITERAL|STRING_LITERAL :");
            SyntaxGrammarProcessor.AddRule("LABEL_DEFAULT", false, "`default` :");

            // Expressions
            SyntaxGrammarProcessor.AddRule("EXPRESSION", false, "NUMBER_LITERAL|STRING_LITERAL");
            SyntaxGrammarProcessor.AddRule("EXPRESSION", false, "VECTOR");
            SyntaxGrammarProcessor.AddRule("EXPRESSION", false, "FUNCTION_CALL");
            SyntaxGrammarProcessor.AddRule("EXPRESSION", false, "( EXPRESSION )");
            SyntaxGrammarProcessor.AddRule("EXPRESSION", false, "VARIABLE");
            SyntaxGrammarProcessor.AddRule("EXPRESSION", false, "EXPRESSION OPERATOR_BINARY EXPRESSION");
            SyntaxGrammarProcessor.AddRule("EXPRESSION", false, "OPERATOR_UNARY EXPRESSION");
            SyntaxGrammarProcessor.AddRule("EXPRESSION", false, "ASSIGNMENT");
            SyntaxGrammarProcessor.AddRule("EXPRESSION", false, "KEYWORD");
            SyntaxGrammarProcessor.AddRule("EXPRESSION", false, "EXPLICIT_FUNCTION_IDENTIFIER");

            // Additional operators
            SyntaxGrammarProcessor.AddRule("OPERATOR_BINARY", false, "OPERATOR_AND");
            SyntaxGrammarProcessor.AddRule("OPERATOR_AND", false, "&&");
            SyntaxGrammarProcessor.AddRule("OPERATOR_BINARY", false, "OPERATOR_OR");
            SyntaxGrammarProcessor.AddRule("OPERATOR_OR", false, "||");

            // Value types
            SyntaxGrammarProcessor.AddRule("PATH", false, "OPTIONAL \\ IDENTIFIER");
            SyntaxGrammarProcessor.AddRule("PATH", false, "PATH \\ IDENTIFIER");
            SyntaxGrammarProcessor.AddRule("PATH_PREFIX", false, "OPTIONAL PATH ::");
            SyntaxGrammarProcessor.AddRule("FUNCTION_IDENTIFIER", false, "OPTIONAL PATH_PREFIX IDENTIFIER");
            SyntaxGrammarProcessor.AddRule("FUNCTION_IDENTIFIER", false, "UNBOX_FUNCTION");
            SyntaxGrammarProcessor.AddRule("UNBOX_FUNCTION", false, "[ [ IDENTIFIER ] ]");
            SyntaxGrammarProcessor.AddRule("EXPLICIT_FUNCTION_IDENTIFIER", false, "PATH_PREFIX IDENTIFIER");
            SyntaxGrammarProcessor.AddRule("EXPRESSION_LIST", false, "EXPRESSION_LIST , EXPRESSION_LIST");
            SyntaxGrammarProcessor.AddRule("EXPRESSION_LIST", false, "EXPRESSION");
            SyntaxGrammarProcessor.AddRule("INDEXER_LIST", false, "INDEXER_LIST INDEXER_LIST");
            SyntaxGrammarProcessor.AddRule("INDEXER_LIST", false, "[ EXPRESSION ]");
            SyntaxGrammarProcessor.AddRule("VARIABLE", false, "VARIABLE INDEXER_LIST");
            SyntaxGrammarProcessor.AddRule("VARIABLE", false, "VARIABLE . IDENTIFIER");
            SyntaxGrammarProcessor.AddRule("VARIABLE", false, "IDENTIFIER");
            SyntaxGrammarProcessor.AddRule("VECTOR", false, "( EXPRESSION , EXPRESSION , EXPRESSION )");
            SyntaxGrammarProcessor.AddRule("VECTOR", false, "( EXPRESSION , EXPRESSION )");

            // Value keywords
            SyntaxGrammarProcessor.AddRule("KEYWORD", false, "`true`");
            SyntaxGrammarProcessor.AddRule("KEYWORD", false, "`false`");
            SyntaxGrammarProcessor.AddRule("KEYWORD", false, "`undefined`");
            SyntaxGrammarProcessor.AddRule("KEYWORD", false, "[]");

            // Add all operators
            SyntaxGrammarProcessor.AddRules("OPERATOR_BINARY", false, EarleOperators.BinaryOperators.OrderByDescending(o => o.Length));
            SyntaxGrammarProcessor.AddRules("OPERATOR_UNARY", false, EarleOperators.UnaryOperators.OrderByDescending(o => o.Length));
            SyntaxGrammarProcessor.AddRules("OPERATOR_ASSIGNMENT", false, EarleOperators.AssignmentOperators.OrderByDescending(o => o.Length));
            SyntaxGrammarProcessor.AddRules("OPERATOR_MOD_ASSIGNMENT", false, EarleOperators.AssignmentModOperators.OrderByDescending(o => o.Length));
        }
    }
}