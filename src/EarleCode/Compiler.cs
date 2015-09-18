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
using System.Diagnostics;
using EarleCode.Blocks;
using EarleCode.Grammar;
using EarleCode.Parsers;
using EarleCode.Tokens;

namespace EarleCode
{
    
    public class Compiler : ICompiler
    {
        private readonly GrammarProcessor _functionGrammar = new GrammarProcessor
        {
            // File contents grammar.
            ["FUNCTION_DECLARATION"] = "IDENTIFIER ( OPTIONAL IDENTIFIER_LIST )",
            ["IDENTIFIER_LIST"] = "IDENTIFIER_LIST , IDENTIFIER_LIST",
            ["IDENTIFIER_LIST"] = "IDENTIFIER",
        };

        public GrammarProcessor Grammar { get; } = new GrammarProcessor
        {
            // Function body grammar.

            // Level 1: Statements
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
            ["FUNCTION_CALL"] = "FUNCTION_IDENTIFIER ( OPTIONAL EXPRESSION_LIST )",

            // Level 2: Expressions
            ["EXPRESSION"] = "FUNCTION_CALL",
            ["EXPRESSION"] = "( EXPRESSION )",
            ["EXPRESSION"] = "EXPRESSION OPERATOR EXPRESSION",
            ["EXPRESSION"] = "OPERATOR_UNARY EXPRESSION",
            ["EXPRESSION"] = "ASSIGNMENT_UNARY",
            ["EXPRESSION"] = "ASSIGNMENT",

            // Level 2 cont: Value types
            ["EXPRESSION"] = "VARIABLE", // variable value
            ["EXPRESSION"] = "FUNCTION_IDENTIFIER", // function reference
            ["EXPRESSION"] = "KEYWORD",
            ["EXPRESSION"] = "VECTOR",
            ["EXPRESSION"] = "NUMBER_LITERAL|STRING_LITERAL",

            // Level 3: Grammar strings
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

            // Level 4: Operators/keywords
            ["KEYWORD"] = "`true`",
            ["KEYWORD"] = "`false`",
            ["KEYWORD"] = "`null`",
            ["OPERATOR"] = "||",
            ["OPERATOR"] = "&&",
//            ["OPERATOR"] = "<<",
//            ["OPERATOR"] = ">>",
            ["OPERATOR"] = "<",
            ["OPERATOR"] = ">",
            ["OPERATOR"] = "<=",
            ["OPERATOR"] = ">=",
            ["OPERATOR"] = "==",
            ["OPERATOR"] = "!=",
            ["OPERATOR"] = "+",
            ["OPERATOR"] = "-",
            ["OPERATOR"] = "*",
            ["OPERATOR"] = "/",
//            ["OPERATOR"] = "^",
            ["OPERATOR_UNARY"] = "+",
            ["OPERATOR_UNARY"] = "-",
            ["OPERATOR_UNARY"] = "!",
            ["OPERATOR_UNARY"] = "~",
            ["OPERATOR_MOD_UNARY"] = "++",
            ["OPERATOR_MOD_UNARY"] = "--",
        };

        private readonly Dictionary<string, IParser> _parsers = new Dictionary<string, IParser>
        {
            ["FUNCTION_CALL"] = new FunctionCallParser(),
            ["STATEMENT_IF"] = new StatementIfParser(),
            ["STATEMENT_WHILE"] = new StatementWhileParser(),
            ["STATEMENT_FOR"] = new StatementForParser(),
            ["STATEMENT_RETURN"] = new StatementReturnParser(),
            ["STATEMENT_WAIT"] = new StatementWaitParser(),
            ["STATEMENT_END"] = new NopParser(),
            ["ASSIGNMENT"] = new AssignmentExpressionParser(),
            ["ASSIGNMENT_UNARY"] = new AssignmentUnaryExpressionParser(),
        };
        
        /// <summary>
        /// Compiles the specified file name / script.
        /// </summary>
        /// <param name="runtime">The runtime.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="script">The script.</param>
        /// <returns></returns>
        public virtual EarleFile Compile(Runtime runtime, string fileName, string script)
        {
            if (fileName == null) throw new ArgumentNullException(nameof(fileName));
            if (script == null) throw new ArgumentNullException(nameof(script));

            var tokenizer = new Tokenizer(fileName, script);
            var functionParser = new FunctionParser();

            var file = new EarleFile(runtime, fileName);

            if (!tokenizer.MoveNext())
                return file;

            do
            {
                var match = _functionGrammar.GetMatch(tokenizer);
                if (match != "FUNCTION_DECLARATION")
                    throw new ParseException(tokenizer.Current, $"Expected function, found {match} `{tokenizer.Current.Value}`");

                var function = functionParser.Parse(this, file, tokenizer);

                file.AddFunction(function.Name, function);
            } while (tokenizer.MoveNext());

            return file;
        }

        /// <summary>
        /// Compiles a block of code in the specified block.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="tokenizer">The tokenizer.</param>
        public virtual void CompileBlock(IBlock target, ITokenizer tokenizer)
        {
            foreach (var block in Compile(target, tokenizer))
                target.AddBlock(block);
        }


        /// <summary>
        /// Compiles a block of code within the specified script scope.
        /// </summary>
        /// <param name="scriptScope">The script scope.</param>
        /// <param name="tokenizer">The tokenizer.</param>
        /// <returns></returns>
        public virtual IEnumerable<IBlock> Compile(IScriptScope scriptScope, ITokenizer tokenizer)
        {
            if (tokenizer == null) throw new ArgumentNullException(nameof(tokenizer));

            var multiLine = false;
            if (tokenizer.Current.Is(TokenType.Token, "{"))
            {
                multiLine = true;
                tokenizer.AssertMoveNext();
            }

            do
            {
                var parserName = Grammar.GetMatch(tokenizer);
                
                IParser parser;
                _parsers.TryGetValue(parserName, out parser);
                
                if (parser == null)
                    throw new Exception($"Expected function definition, found {parserName} {tokenizer.Current}.");

                var result = parser.Parse(this, scriptScope, tokenizer);

                if (result != null)
                    yield return result;

                tokenizer.AssertMoveNext();
            } while (multiLine && !tokenizer.Current.Is(TokenType.Token, "}"));

            if (multiLine)
            {
                tokenizer.AssertToken(TokenType.Token, "}");
            }
        }
    }
}