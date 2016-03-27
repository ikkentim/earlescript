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

using EarleCode.Retry.Grammar;

namespace EarleCode.Retry
{
    public partial class Compiler
    {
        private readonly GrammarProcessor _fileGrammarProcessor = new GrammarProcessor
        {
            // File contents grammar.
            ["FUNCTION_DECLARATION"] = "IDENTIFIER ( OPTIONAL IDENTIFIER_LIST )",
            ["IDENTIFIER_LIST"] = "IDENTIFIER_LIST , IDENTIFIER_LIST",
            ["IDENTIFIER_LIST"] = "IDENTIFIER",
        };

        /// <summary>
        ///     Gets the grammar associated with this compiler.
        /// </summary>
        public GrammarProcessor SyntaxGrammarProcessor { get; } = new GrammarProcessor
        {
            // Statements
            ["STATEMENT_IF"] = "`if` ( EXPRESSION )",
            ["STATEMENT_DO"] = "`do`",
            ["STATEMENT_WHILE"] = "`while` ( EXPRESSION )",
            ["STATEMENT_FOR"] = "`for` ( OPTIONAL ASSIGNMENT ; OPTIONAL EXPRESSION ; OPTIONAL ASSIGNMENT )",
            ["STATEMENT_RETURN"] = "`return` OPTIONAL EXPRESSION ;",
            ["STATEMENT_WAIT"] = "`wait` NUMBER_LITERAL ;",
            ["STATEMENT_END"] = ",",
            ["ASSIGNMENT_UNARY"] = "VARIABLE OPERATOR_MOD_UNARY",
            ["ASSIGNMENT_UNARY"] = "OPERATOR_MOD_UNARY VARIABLE",
            ["ASSIGNMENT"] = "VARIABLE = EXPRESSION",
            ["ASSIGNMENT"] = "ASSIGNMENT_UNARY",
            ["FUNCTION_CALL"] = "OPTIONAL VARIABLE FUNCTION_CALL_PART",
            ["FUNCTION_CALL_PART"] = "FUNCTION_IDENTIFIER ( OPTIONAL EXPRESSION_LIST )",

            // Expressions
//            ["EXPRESSION"] = "FUNCTION_CALL",
//            ["EXPRESSION"] = "( EXPRESSION )",
            ["EXPRESSION"] = "EXPRESSION OPERATOR EXPRESSION",
            ["EXPRESSION"] = "OPERATOR_UNARY EXPRESSION",
//            ["EXPRESSION"] = "ASSIGNMENT_UNARY",
//            ["EXPRESSION"] = "ASSIGNMENT",
//            ["EXPRESSION"] = "FUNCTION_IDENTIFIER", // function reference
            ["EXPRESSION"] = "KEYWORD",//v
//            ["EXPRESSION"] = "VECTOR",
            ["EXPRESSION"] = "VARIABLE",
            ["EXPRESSION"] = "NUMBER_LITERAL|STRING_LITERAL",//v

            // Value types
            ["TARGET_VARIABLE"] = "VARIABLE",
            ["PATH"] = "\\IDENTIFIER",
            ["PATH"] = "PATH\\IDENTIFIER",
            ["PATH_PREFIX"] = "OPTIONAL PATH ::",
            ["FUNCTION_IDENTIFIER"] = "OPTIONAL PATH_PREFIX IDENTIFIER",
            ["EXPRESSION_LIST"] = "EXPRESSION_LIST , EXPRESSION_LIST",
            ["EXPRESSION_LIST"] = "EXPRESSION",
            ["INDEXER_LIST"] = "INDEXER_LIST INDEXER_LIST",
            ["INDEXER_LIST"] = "[ EXPRESSION ]",
            ["VARIABLE"] = "IDENTIFIER OPTIONAL INDEXER_LIST",
            ["VECTOR"] = "( EXPRESSION , EXPRESSION , EXPRESSION )",

            // Value keywords
            ["KEYWORD"] = "`true`",
            ["KEYWORD"] = "`false`",
            ["KEYWORD"] = "`null`",

            // Operators
            ["OPERATOR"] = "||",
            ["OPERATOR"] = "&&",
            //            ["OPERATOR"] = "<<",
            //            ["OPERATOR"] = ">>",
            ["OPERATOR"] = "<=",
            ["OPERATOR"] = ">=",
            ["OPERATOR"] = "==",
            ["OPERATOR"] = "!=",
            ["OPERATOR"] = "<",
            ["OPERATOR"] = ">",
            ["OPERATOR"] = "+",
            ["OPERATOR"] = "-",
            ["OPERATOR"] = "*",
            ["OPERATOR"] = "/",
            //            ["OPERATOR"] = "^",
            ["OPERATOR_UNARY"] = "+",
            ["OPERATOR_UNARY"] = "-",
            ["OPERATOR_UNARY"] = "!",
//            ["OPERATOR_UNARY"] = "~",
            ["OPERATOR_MOD_UNARY"] = "++",
            ["OPERATOR_MOD_UNARY"] = "--",
            ["END_BLOCK"] = "}"
        };
    }
}