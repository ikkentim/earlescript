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

using System;
using System.Collections.Generic;
using EarleCode.Instructions;
using EarleCode.Lexing;
using EarleCode.Parsers;
using EarleCode.Utilities;

namespace EarleCode
{
    public partial class Compiler
    {
        private readonly Dictionary<string, IParser> _parsers = new Dictionary<string, IParser>
        {
            ["FUNCTION_CALL"] = new StatementCallParser(),
            ["STATEMENT_IF"] = new StatementIfParser(),
            ["STATEMENT_WHILE"] = new StatementWhileParser(),
            ["STATEMENT_FOR"] = new StatementForParser(),
            ["STATEMENT_RETURN"] = new StatementReturnParser(),
//            ["STATEMENT_WAIT"] = new StatementWaitParser(),
            ["ASSIGNMENT"] = new StatementAssignmentParser()
        };

        private readonly Runtime _runtime;

        public Compiler(Runtime runtime)
        {
            if (runtime == null) throw new ArgumentNullException(nameof(runtime));
            _runtime = runtime;

            InitializeGrammarProcessor();
        }

        #region Compiling

        public virtual EarleFile CompileFile(string fileName, string script)
        {
            if (fileName == null) throw new ArgumentNullException(nameof(fileName));
            if (script == null) throw new ArgumentNullException(nameof(script));

            fileName = fileName.ToLower();
            var tokenizer = new Lexer(fileName, script);
            var file = new EarleFile(_runtime, fileName);

            // Recursively look foor function declarations
            while (tokenizer.MoveNext())
            {
                var match = _fileGrammarProcessor.GetMatch(tokenizer);
                if (match != "FUNCTION_DECLARATION")
                    throw new ParseException(tokenizer.Current,
                        $"Expected function, found {match} `{tokenizer.Current.Value}`");

                // Compile the function and add it to the file
                file.AddFunction(CompileFunction(tokenizer, file));
            }

            return file;
        }

        public EarleFunction CompileFunction(ILexer lexer, EarleFile file)
        {
            if (lexer == null) throw new ArgumentNullException(nameof(lexer));

            var name = lexer.Current.Value.ToLower();
            var parameters = new List<string>();

            lexer.AssertMoveNext();
            lexer.SkipToken(TokenType.Token, "(");

            while (!lexer.Current.Is(TokenType.Token, ")"))
            {
                lexer.AssertToken(TokenType.Identifier);

                if (parameters.Contains(lexer.Current.Value))
                    throw new ParseException(lexer.Current, $"Duplicate parameter name in function \"{name}\"");

                parameters.Add(lexer.Current.Value);
                lexer.AssertMoveNext();

                if (lexer.Current.Is(TokenType.Token, ")"))
                    break;

                lexer.SkipToken(TokenType.Token, ",");
            }

            lexer.SkipToken(TokenType.Token, ")");

            var function = new EarleFunction(file, name, parameters.ToArray(), Compile(lexer, file, true));

            PrintCompiledPCode(function);

            return function;
        }

        public byte[] Compile(ILexer lexer, EarleFile file, bool mustReturn)
        {
            var result = new List<byte>();
            var didReturnAnyValue = false;
            var multiLine = false;
            Token lastToken;

            if (lexer.Current.Is(TokenType.Token, "{"))
            {
                multiLine = true;
                lexer.AssertMoveNext();
            }

            result.Add((byte) OpCode.PushScope);

            do
            {
                if(multiLine && lexer.Current.Is(TokenType.Token, "}"))
                    break;
                
                var parserName = SyntaxGrammarProcessor.GetMatch(lexer, true);
                if (parserName == null)
                    throw new ParseException(lexer.Current, $"Expected statement, found token `{lexer.Current.Value}`");

                IParser parser;
                if (!_parsers.TryGetValue(parserName, out parser))
                    throw new ParseException(lexer.Current,
                        $"Expected statement, found {parserName.ToLower()} `{lexer.Current.Value}`");

                if (parser is StatementReturnParser)
                    didReturnAnyValue = true;

                result.AddRange(parser.Parse(_runtime, file, lexer));

                lastToken = lexer.Current;

                lexer.SkipToken(TokenType.Token, ";");
            } while (multiLine && !lexer.Current.Is(TokenType.Token, "}"));

            if (multiLine)
                lexer.AssertToken(TokenType.Token, "}");
            else
                lexer.Push(lastToken);

            if (!didReturnAnyValue && mustReturn)
                result.Add((byte) OpCode.PushUndefined);

            result.Add((byte) OpCode.PopScope);

            return result.ToArray();
        }

        #endregion

        #region Debugging

        public void PrintCompiledPCode(EarleFunction function)
        {
            if (function == null) throw new ArgumentNullException(nameof(function));

            Console.WriteLine($"Function {function} compiled to:");
            PrintCompiledPCode(function.PCode);
            Console.WriteLine();
        }

        public void PrintCompiledPCode(byte[] pCode)
        {
            for (var index = 0; index < pCode.Length; index++)
            {
                var p = pCode[index];
                var e = (OpCode) p;
                var a = e.GetCustomAttribute<OpCodeAttribute>();

                Console.WriteLine(a.BuildString(pCode, ref index));
            }
        }

        #endregion
    }
}