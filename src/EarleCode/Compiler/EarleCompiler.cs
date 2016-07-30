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
using System.Linq;
using EarleCode.Compiler.Lexing;
using EarleCode.Compiler.Parsers;
using EarleCode.Runtime;
using EarleCode.Runtime.Instructions;
using EarleCode.Utilities;

namespace EarleCode.Compiler
{
    internal partial class EarleCompiler
    {
        private readonly Dictionary<string, IParser> _parsers = new Dictionary<string, IParser>
        {
            ["FUNCTION_CALL"] = new StatementCallParser(),
            ["ASSIGNMENT"] = new StatementAssignmentParser(),
            ["STATEMENT_IF"] = new StatementIfParser(),
            ["STATEMENT_WHILE"] = new StatementWhileParser(),
            ["STATEMENT_FOR"] = new StatementForParser(),
            ["STATEMENT_RETURN"] = new StatementReturnParser(),
            ["STATEMENT_WAIT"] = new StatementWaitParser(),
            ["STATEMENT_SWITCH"] = new StatementSwitchParser()
        };

        private readonly EarleRuntime _runtime;

        public EarleCompiler(EarleRuntime runtime)
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
            var lexer = new Lexer(fileName, script);
            var file = new EarleFile(_runtime, fileName);

            lexer.MoveNext();

            // Recursively look foor function declarations
            while (lexer.Current != null)
            {
                var match = _fileGrammarProcessor.GetMatch(lexer, true);

                switch(match)
                {
                    case "FUNCTION_DECLARATION":
                        // Compile the function and add it to the file
                        file.AddFunction(CompileFunction(lexer, file));
                        break;
                    case "INCLUDE":
                        lexer.SkipToken(TokenType.Token, "#");
                        lexer.SkipToken(TokenType.Identifier, "include");

                        var identifier = !lexer.Current.Is(TokenType.Token, "\\");
                        var path = identifier ? "\\" : string.Empty;
                        do
                        {
                            // check syntax
                            if(identifier)
                                lexer.AssertToken(TokenType.Identifier);
                            else
                                lexer.AssertToken(TokenType.Token, "\\");
                            identifier = !identifier;

                            path += lexer.Current.Value;

                            lexer.AssertMoveNext();
                        } while(!lexer.Current.Is(TokenType.Token, ";"));

                        lexer.SkipToken(TokenType.Token, ";");

                        file.IncludeFile(path);
                        break;
                    default:
                        throw new ParseException(lexer.Current,
                            $"Expected function, found {match} `{lexer.Current.Value}`");
                }
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

            var function = new EarleFunction(file, name, parameters.ToArray(), Compile(lexer, file, EarleCompileOptions.Method).PCode);

            PrintCompiledPCode(function);

            return function;
        }

        public CompiledBlock Compile(ILexer lexer, EarleFile file, EarleCompileOptions options)
        {
            var pCode = new List<byte>();
            var breaks = new List<int>();
            var continues = new List<int>();

            var didReturnAnyValue = false;
            var multiLine = false;

            if (lexer.Current.Is(TokenType.Token, "{"))
            {
                multiLine = true;
                lexer.AssertMoveNext();
            }

            var requiresScope = false;

            do
            {
                if(multiLine && lexer.Current.Is(TokenType.Token, "}"))
                    break;
                
                var parserName = SyntaxGrammarProcessor.GetMatch(lexer, true);
                if (parserName == null)
                    throw new ParseException(lexer.Current, $"Expected statement, found token `{lexer.Current.Value}`");

                if(parserName == "STATEMENT_BREAK" && options.HasFlag(EarleCompileOptions.CanBreak))
                {
                    lexer.SkipToken(TokenType.Identifier, "break");
                    lexer.SkipToken(TokenType.Token, ";");

                    breaks.Add(pCode.Count);
                    pCode.Add((byte)OpCode.Jump);
                    pCode.AddRange(ArrayUtility.Repeat((byte) 0xaa, 4));
                    didReturnAnyValue = false;

                    if(options.HasFlag(EarleCompileOptions.EnforceMultiline))
                        break;
                }
                else if(parserName == "STATEMENT_CONTINUE" && options.HasFlag(EarleCompileOptions.CanContinue))
                {
                    lexer.SkipToken(TokenType.Identifier, "continue");
                    lexer.SkipToken(TokenType.Token, ";");

                    continues.Add(pCode.Count);
                    pCode.Add((byte)OpCode.Jump);
                    pCode.AddRange(ArrayUtility.Repeat((byte)0xaa, 4));
                    didReturnAnyValue = false;

                    if(options.HasFlag(EarleCompileOptions.EnforceMultiline))
                        break;
                }
                else
                {
                    IParser parser;
                    if(!_parsers.TryGetValue(parserName, out parser))
                        throw new ParseException(lexer.Current,
                            $"Expected statement, found {parserName.ToLower()} `{lexer.Current.Value}`");

                    var block = parser.Parse(_runtime, file, lexer, options);

                    breaks.AddRange(block.Breaks.Select(b => b + pCode.Count));
                    continues.AddRange(block.Continues.Select(c => c + pCode.Count));
                    requiresScope = requiresScope || block.RequiresScope;
                    pCode.AddRange(block.PCode);


                    if(parser is ISimpleStatement)
                        lexer.SkipToken(TokenType.Token, ";");

                    didReturnAnyValue = parser is StatementReturnParser;

                    if(didReturnAnyValue && options.HasFlag(EarleCompileOptions.EnforceMultiline))
                        break;
                }

                if(options.HasFlag(EarleCompileOptions.EnforceMultiline) && SyntaxGrammarProcessor.MatchStartsWith(lexer, "LABEL_"))
                    break;
                
            } while (options.HasFlag(EarleCompileOptions.EnforceMultiline) || (multiLine && !lexer.Current.Is(TokenType.Token, "}")));

            if(multiLine)
            {
                lexer.AssertToken(TokenType.Token, "}");
                lexer.MoveNext();
            }

            if (!didReturnAnyValue && options.HasFlag(EarleCompileOptions.MustReturn))
                pCode.Add((byte) OpCode.PushUndefined);

            if(requiresScope)
            {
                pCode.Insert(0, (byte)OpCode.PushScope);
                pCode.Add((byte)OpCode.PopScope);
            }

            return new CompiledBlock(pCode.ToArray(), breaks.ToArray(), continues.ToArray(), false);
        }

        #endregion

        #region Debugging

        public void PrintCompiledPCode(EarleFunction function)
        {
            if (function == null) throw new ArgumentNullException(nameof(function));

            Console.WriteLine($"Function {function.File.Name}::{function.Name} compiled to:");
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