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
using System.Linq;
using Earle.Blocks;
using Earle.Grammars;
using Earle.Parsers;
using Earle.Tokens;

namespace Earle
{
    public class Compiler
    {
        private static readonly IParser[] Parsers =
        {
            new FunctionCallParser(),
            new EndExpressionParser(),
            new ReturnParser(),
            new AssignmentParser(),
            new IfStatementParser(),
            new WhileStatementParser(),
            new ForStatementParser()
        };

        public static readonly Grammar Grammar = new Grammar();
        public static readonly Grammar FunctionGrammar = new Grammar();
        private readonly Engine _engine;

        static Compiler()
        {
            Grammar.Add("STATEMENT_IF", "`if` ( EXPRESSION )");
//            Grammar.Add("STATEMENT_DO", "`do`");
            Grammar.Add("STATEMENT_WHILE", "`while` ( EXPRESSION )");
            Grammar.Add("STATEMENT_FOR", "`for` ( OPTIONAL ASSIGNMENT ; OPTIONAL EXPRESSION ; OPTIONAL ASSIGNMENT )");
            Grammar.Add("STATEMENT_RETURN", "`return` OPTIONAL EXPRESSION ;");
            Grammar.Add("STATEMENT_END", ";");

            Grammar.Add("ASSIGNMENT", "IDENTIFIER OPTIONAL INDEXER_LIST = EXPRESSION");
            Grammar.Add("ASSIGNMENT", "IDENTIFIER OPERATOR_POST_UNARY");
//            Grammar.Add("ASSIGNMENT", "INDEXED_IDENTIFIER OPERATOR_POST_UNARY");

            Grammar.Add("FUNCTION_CALL", "OPTIONAL PATH_PREFIX IDENTIFIER ( OPTIONAL EXPRESSION_LIST )");

            Grammar.Add("EXPRESSION", "( EXPRESSION )");
            Grammar.Add("EXPRESSION", "IDENTIFIER INDEXER_LIST");
            Grammar.Add("EXPRESSION", "EXPRESSION OPERATOR EXPRESSION");
            Grammar.Add("EXPRESSION", "OPERATOR_UNARY EXPRESSION");
            Grammar.Add("EXPRESSION", "IDENTIFIER|NUMBER_LITERAL|STRING_LITERAL");
            Grammar.Add("EXPRESSION", "FUNCTION_CALL");
            Grammar.Add("EXPRESSION", "`true`");
            Grammar.Add("EXPRESSION", "`false`");
            Grammar.Add("EXPRESSION", "`null`");

            // Helpers below

            Grammar.Add("PATH", "\\IDENTIFIER");
            Grammar.Add("PATH", "PATH\\IDENTIFIER");
            Grammar.Add("PATH_PREFIX", "PATH ::");

            Grammar.Add("EXPRESSION_LIST", "EXPRESSION_LIST , EXPRESSION_LIST");
            Grammar.Add("EXPRESSION_LIST", "EXPRESSION");

            Grammar.Add("INDEXER_LIST", "INDEXER_LIST INDEXER_LIST");
            Grammar.Add("INDEXER_LIST", "[ EXPRESSION ]");

            Grammar.Add("OPERATOR", "||");
            Grammar.Add("OPERATOR", "&&");
            Grammar.Add("OPERATOR", "<<");
            Grammar.Add("OPERATOR", ">>");
            Grammar.Add("OPERATOR", "<");
            Grammar.Add("OPERATOR", ">");
            Grammar.Add("OPERATOR", "<=");
            Grammar.Add("OPERATOR", ">=");
            Grammar.Add("OPERATOR", "==");
            Grammar.Add("OPERATOR", "!=");
            Grammar.Add("OPERATOR", "+");
            Grammar.Add("OPERATOR", "-");
            Grammar.Add("OPERATOR", "*");
            Grammar.Add("OPERATOR", "/");
            Grammar.Add("OPERATOR", "^");

            Grammar.Add("OPERATOR_UNARY", "+");
            Grammar.Add("OPERATOR_UNARY", "-");
            Grammar.Add("OPERATOR_UNARY", "!");
            Grammar.Add("OPERATOR_UNARY", "~");

            Grammar.Add("OPERATOR_POST_UNARY", "++");
            Grammar.Add("OPERATOR_POST_UNARY", "--");

            FunctionGrammar.Add("FUNCTION_DECLARATION", "IDENTIFIER ( OPTIONAL IDENTIFIER_LIST )");
            FunctionGrammar.Add("IDENTIFIER_LIST", "IDENTIFIER_LIST , IDENTIFIER_LIST");
            FunctionGrammar.Add("IDENTIFIER_LIST", "IDENTIFIER");
        }

        public Compiler(Engine engine)
        {
            _engine = engine;
        }

        public IEnumerable<Block> CompileCodeBlock(Block parent, Tokenizer tokenizer)
        {
            var singleStatement = !tokenizer.Current.Is(TokenType.Token, "{");

            // If this code block does not contain a single statement, skip the `{`.
            if (!singleStatement)
            {
                if (!tokenizer.MoveNext())
                    new CompilerException("Unexpected end of file");
            }

            // Find matching parsers until end of code block.
            while (tokenizer.Current != null && (singleStatement || !tokenizer.Current.Is(TokenType.Token, "}")))
            {
                var match = Grammar.GetMatch(tokenizer);
                var parser = Parsers.FirstOrDefault(p => match == p.ParserRule);

                if (parser == null)
                    throw new CompilerException(tokenizer.Current,
                        string.Format("Unexpected {0}", tokenizer.Current.Type.ToUpperString()));

                // Parse the tokens and return the resulting block.
                var block = parser.Parse(parent, tokenizer);

                if (parser.RequiresBlock)
                    foreach (var sub in CompileCodeBlock(block, tokenizer))
                        block.AddBlock(sub);

                yield return block;

                if (tokenizer.Current.Is(TokenType.Token, ";"))
                    tokenizer.MoveNext();

                if (singleStatement)
                    yield break;
            }

            if (singleStatement)
                throw new CompilerException("Unexpected end of file");

            if (!tokenizer.Current.Is(TokenType.Token, "}"))
            {
                throw new CompilerException("Unknown error");
            }

            tokenizer.MoveNext();
        }

        public EarleFile Compile(string path, string input)
        {
            if (path == null) throw new ArgumentNullException("path");
            if (input == null) throw new ArgumentNullException("input");

            var tokenizer = new Tokenizer(path, input);
            var functionParser = new FunctionParser();

            var file = new EarleFile(_engine, path);

            while (tokenizer.Current != null)
            {
                var match = FunctionGrammar.GetMatch(tokenizer);
                if (match != functionParser.ParserRule)
                    throw new CompilerException(tokenizer.Current,
                        string.Format("Expected function, found {1} `{0}`", tokenizer.Current.Value, match));

                var function = functionParser.Parse(file, tokenizer);

                foreach (var block in CompileCodeBlock(function, tokenizer))
                    function.AddBlock(block);

                file.Functions.Add(function);
            }

            return file;
        }
    }
}