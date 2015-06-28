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
            new IfStatementParser()
        };

        public static readonly Grammar Grammar = new Grammar();
        public static readonly Grammar FunctionGrammar = new Grammar();
        private readonly Engine _engine;

        static Compiler()
        {
            Grammar.AddProductionRule("STATEMENT_IF", "`if` ( EXPRESSION )");
            Grammar.AddProductionRule("STATEMENT_WHILE", "`while` ( EXPRESSION )");
            Grammar.AddProductionRule("STATEMENT_DO", "`do`");
            Grammar.AddProductionRule("STATEMENT_FOR",
                "`for` ( OPTIONAL ASSIGNMENT ; OPTIONAL EXPRESSION ; OPTIONAL EXPRESSION )");
            Grammar.AddProductionRule("STATEMENT_END", ";");
            Grammar.AddProductionRule("STATEMENT_RETURN", "`return` OPTIONAL EXPRESSION ;");

            Grammar.AddProductionRule("ASSIGNMENT", "IDENTIFIER = EXPRESSION ;");

            Grammar.AddProductionRule("FUNCTION_CALL", "OPTIONAL PATH_PREFIX IDENTIFIER ( OPTIONAL EXPRESSION_LIST )");

            Grammar.AddProductionRule("PATH", "\\IDENTIFIER");
            Grammar.AddProductionRule("PATH", "PATH\\IDENTIFIER");
            Grammar.AddProductionRule("PATH_PREFIX", "PATH ::");

            Grammar.AddProductionRule("EXPRESSION", "( EXPRESSION )");
            Grammar.AddProductionRule("EXPRESSION", "EXPRESSION OPERATOR EXPRESSION");
            Grammar.AddProductionRule("EXPRESSION", "OPERATOR_UNARY EXPRESSION");
            Grammar.AddProductionRule("EXPRESSION", "IDENTIFIER|NUMBER_LITERAL|STRING_LITERAL");
            Grammar.AddProductionRule("EXPRESSION", "FUNCTION_CALL");

            Grammar.AddProductionRule("EXPRESSION_LIST", "EXPRESSION_LIST , EXPRESSION_LIST");
            Grammar.AddProductionRule("EXPRESSION_LIST", "EXPRESSION");

            Grammar.AddProductionRule("OPERATOR", "||");
            Grammar.AddProductionRule("OPERATOR", "&&");
            Grammar.AddProductionRule("OPERATOR", "<<");
            Grammar.AddProductionRule("OPERATOR", ">>");
            Grammar.AddProductionRule("OPERATOR", "<");
            Grammar.AddProductionRule("OPERATOR", ">");
            Grammar.AddProductionRule("OPERATOR", "<=");
            Grammar.AddProductionRule("OPERATOR", ">=");
            Grammar.AddProductionRule("OPERATOR", "==");
            Grammar.AddProductionRule("OPERATOR", "!=");
            Grammar.AddProductionRule("OPERATOR", "+");
            Grammar.AddProductionRule("OPERATOR", "-");
            Grammar.AddProductionRule("OPERATOR", "*");
            Grammar.AddProductionRule("OPERATOR", "/");
            Grammar.AddProductionRule("OPERATOR", "^");

            Grammar.AddProductionRule("OPERATOR_UNARY", "+");
            Grammar.AddProductionRule("OPERATOR_UNARY", "-");
            Grammar.AddProductionRule("OPERATOR_UNARY", "!");
            Grammar.AddProductionRule("OPERATOR_UNARY", "~");

            FunctionGrammar.AddProductionRule("FUNCTION_DECLARATION", "IDENTIFIER ( OPTIONAL IDENTIFIER_LIST )");
            FunctionGrammar.AddProductionRule("IDENTIFIER_LIST", "IDENTIFIER_LIST , IDENTIFIER_LIST");
            FunctionGrammar.AddProductionRule("IDENTIFIER_LIST", "IDENTIFIER");
        }

        public Compiler(Engine engine)
        {
            _engine = engine;
        }

        public IEnumerable<Block> CompileCodeBlock(Block parent, Tokenizer tokenizer)
        {
            var singleStatement = tokenizer.Current.Type != TokenType.Token || tokenizer.Current.Value != "{";

            // If this code block does not contain a single statement, skip the `{`.
            if (!singleStatement)
            {
                if (!tokenizer.MoveNext())
                    new CompilerException("Unexpected end of file");
            }
            // Find matching parsers until end of code block.
            while (tokenizer.Current != null &&
                   (singleStatement || !(tokenizer.Current.Type == TokenType.Token && tokenizer.Current.Value == "}")))
            {
                var found = false;

                var parser = Parsers.FirstOrDefault(p => Grammar.GetMatch(tokenizer) == p.ParserRule);

                if (parser == null)
                    throw new CompilerException(tokenizer.Current,
                        string.Format("Unexpected {0}", tokenizer.Current.Type.ToUpperString()));


                // Parse the tokens and return the resulting block.
                var block = parser.Parse(parent, tokenizer);

                if (parser.RequiresBlock)
                    foreach (var sub in CompileCodeBlock(block, tokenizer))
                        block.AddBlock(sub);

                yield return block;

                if (singleStatement)
                    yield break;
            }

            if (singleStatement)
                throw new CompilerException("Unexpected end of file");
        }

        public EarleFile Compile(string path, string input)
        {
            if (path == null) throw new ArgumentNullException("path");
            if (input == null) throw new ArgumentNullException("input");

            var tokenizer = new Tokenizer(path, input);
            var functionParser = new FunctionParser();

            var file = new EarleFile(_engine, path);

            do
            {
                var match = FunctionGrammar.GetMatch(tokenizer);
                if (match != functionParser.ParserRule)
                    throw new CompilerException(tokenizer.Current,
                        string.Format("Expected function, found {1} `{0}`", tokenizer.Current.Value, match));

                var function = functionParser.Parse(file, tokenizer);

                foreach (var block in CompileCodeBlock(function, tokenizer))
                    function.AddBlock(block);

                file.Functions.Add(function);
            } while (tokenizer.MoveNext());

            return file;
        }
    }
}