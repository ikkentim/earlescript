using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels;
using EarleCode.Retry.Instructions;
using EarleCode.Retry.Grammar;
using EarleCode.Retry.Parsers;
using EarleCode.Retry.Lexing;

namespace EarleCode.Retry
{
    public partial class Compiler
    {
        private readonly Runtime _runtime;

        private readonly Dictionary<string, IParser> _parsers = new Dictionary<string, IParser>
        {
            ["FUNCTION_CALL"] = new VoidCallParser(),
//            ["STATEMENT_IF"] = new StatementIfParser(),
//            ["STATEMENT_WHILE"] = new StatementWhileParser(),
//            ["STATEMENT_FOR"] = new StatementForParser(),
//            ["STATEMENT_RETURN"] = new StatementReturnParser(),
//            ["STATEMENT_WAIT"] = new StatementWaitParser(),
//            ["STATEMENT_END"] = new NopParser(),
            ["ASSIGNMENT"] = new AssignmentExpressionParser(),
//            ["ASSIGNMENT_UNARY"] = new AssignmentUnaryExpressionParser(),
        };

        public Compiler(Runtime runtime)
        {
            if (runtime == null) throw new ArgumentNullException(nameof(runtime));
            _runtime = runtime;
        }

        public EarleFunction CompileFunction(ILexer lexer, EarleFile file)
        {
            if (lexer == null) throw new ArgumentNullException(nameof(lexer));
            
            var name = lexer.Current.Value;
            //var parameters = new List<string>();

            lexer.AssertMoveNext();
            lexer.SkipToken(TokenType.Token, "(");

            if (!lexer.Current.Is(TokenType.Token, ")"))
                while (true)
                {
                    if (lexer.Current.Is(TokenType.Identifier))
                    {
                        // todo parameters
                        // parameters.Add(lexer.Current.Value);
                    }

                    lexer.SkipToken(TokenType.Identifier);

                    if (!lexer.Current.Is(TokenType.Token, ","))
                        break;

                    lexer.AssertMoveNext();
                }

            lexer.SkipToken(TokenType.Token, ")");

            // TODO: Make EarleFunction needs a scope in which parameters can be stored.
            var func =  new EarleFunction(file, name, new string[0], Compile(lexer, file, true).ToArray());

            // PRINT COMPILER OUTPUT
            Console.WriteLine($"Function {name} compiled to:");
            for (var index = 0; index < func.PCode.Length; index++)
            {
                var p = func.PCode[index];
                var e = (OpCode) p;
                var a = e.GetAttributeOfType<OpCodeAttribute>();

                var str = "";
                var l = new Lexer("descr", a.Format);
                while (l.MoveNext())
                {
                    if (l.Current.Is(TokenType.Token, "$"))
                    {
                        l.AssertMoveNext();
                        l.AssertToken(TokenType.Identifier);
                        
                        switch (l.Current.Value)
                        {
                            case "int":
                                str += BitConverter.ToInt32(func.PCode, index + 1);
                                str += " ";
                                index += 4;
                                break;
                            case "float":
                                str += BitConverter.ToSingle(func.PCode, index + 1);
                                str += " ";
                                index += 4;
                                break;
                            case "string":
                                while (func.PCode[index] != 0)
                                    str += (char) func.PCode[index++];
                                str += " ";
                                break;
                            default:
                                throw new Exception();
                        }
                    }
                    else
                    {
                        str += $"{l.Current.Value} ";
                    }

                }
                Console.WriteLine(str);
            }

            return func;
        }

        public IEnumerable<byte> Compile(ILexer lexer, EarleFile file, bool mustReturn)
        {
            var didReturnAnyValue = false;
            var multiLine = false;
            if (lexer.Current.Is(TokenType.Token, "{"))
            {
                multiLine = true;
                lexer.AssertMoveNext();
            }

            Token lastToken = null;
            do
            {
                var parserName = SyntaxGrammarProcessor.GetMatch(lexer);

                IParser parser;
                _parsers.TryGetValue(parserName, out parser);

                if (parserName == "END_BLOCK")
                    break;

                if (parser == null)
                    throw new Exception($"Expected instruction, found {parserName} {lexer.Current}.");

                var result = parser.Parse(_runtime, this, file, lexer);

                if(result != null)
                    foreach (var r in result)
                        yield return r;

                lastToken = lexer.Current;
                lexer.AssertMoveNext();
            } while (multiLine && !lexer.Current.Is(TokenType.Token, "}"));

            if (multiLine)
                lexer.AssertToken(TokenType.Token, "}");
            else
                lexer.Push(lastToken);

            if (!didReturnAnyValue && mustReturn)
            {
                yield return (byte) OpCode.PushInteger;
                foreach (var r in BitConverter.GetBytes(0))
                    yield return r;
            }
        } 

        /// <summary>
        /// Compiles the specified file name / script.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="script">The script.</param>
        /// <returns></returns>
        public virtual EarleFile CompileFile(string fileName, string script)
        {
            if (fileName == null) throw new ArgumentNullException(nameof(fileName));
            if (script == null) throw new ArgumentNullException(nameof(script));

            var tokenizer = new Lexer(fileName, script);
            var file = new EarleFile(_runtime, fileName);
            
            // Recursively look foor function declarations
            while (tokenizer.MoveNext())
            {
                var match = _fileGrammarProcessor.GetMatch(tokenizer);
                if (match != "FUNCTION_DECLARATION")
                    throw new ParseException(tokenizer.Current, $"Expected function, found {match} `{tokenizer.Current.Value}`");
                
                // Compile the function and add it to the file
                file.AddFunction(CompileFunction(tokenizer, file));
            }

            return file;
        }
    }
}