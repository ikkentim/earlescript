using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting.Channels;
using EarleCode.Instructions;
using EarleCode.Lexing;
using EarleCode.Parsers;
using EarleCode.Grammar;
using EarleCode.Utilities;

namespace EarleCode
{
    public partial class Compiler
    {
        private readonly Runtime _runtime;

        private readonly Dictionary<string, IParser> _parsers = new Dictionary<string, IParser>
        {
            ["FUNCTION_CALL"] = new VoidCallParser(),
            ["STATEMENT_IF"] = new IfStatementParser(),
            ["STATEMENT_WHILE"] = new WhileStatementParser(),
//            ["STATEMENT_FOR"] = new StatementForParser(),
            ["STATEMENT_RETURN"] = new ReturnStatementParser(),
//            ["STATEMENT_WAIT"] = new StatementWaitParser(),
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
            var parameters = new List<string>();

            lexer.AssertMoveNext();
            lexer.SkipToken(TokenType.Token, "(");

            while (!lexer.Current.Is(TokenType.Token, ")"))
            {
                lexer.AssertToken(TokenType.Identifier);
                parameters.Add(lexer.Current.Value);
                lexer.AssertMoveNext();

                if (lexer.Current.Is(TokenType.Token, ")"))
                    break;
                
                lexer.SkipToken(TokenType.Token, ",");
            }

            lexer.SkipToken(TokenType.Token, ")");
            
            var function =  new EarleFunction(file, name, parameters.ToArray(), Compile(lexer, file, true).ToArray());

            PrintCompiledPCode(function);

            return function;
        }

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
                var e = (OpCode)p;
                var a = e.GetAttributeOfType<OpCodeAttribute>();

                Console.WriteLine(a.BuildString(pCode, ref index));
            }
        }

        public IEnumerable<byte> Compile(ILexer lexer, EarleFile file, bool mustReturn)
        {
            var didReturnAnyValue = false;
            var multiLine = false;
            Token lastToken = null;

            if (lexer.Current.Is(TokenType.Token, "{"))
            {
                multiLine = true;
                lexer.AssertMoveNext();
            }

            yield return (byte)OpCode.PushScope;

            do
            {
                var parserName = SyntaxGrammarProcessor.GetMatch(lexer);

                if(parserName == null)
                    throw new ParseException(lexer.Current, "Unexpected token");

                IParser parser;
                _parsers.TryGetValue(parserName, out parser);

                if (parserName == "END_BLOCK")
                    break;

                if (parserName == "STATEMENT_RETURN")
                    didReturnAnyValue = true;

                if (parser == null)
                    throw new ParseException(lexer.Current, $"Expected token, found {parserName} {lexer.Current}");

                var result = parser.Parse(_runtime, file, lexer);

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
                yield return (byte) OpCode.PushNull;

            yield return (byte)OpCode.PopScope;
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