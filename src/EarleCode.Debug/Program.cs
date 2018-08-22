using System;
using System.Linq;
using EarleCode.Compiling;
using EarleCode.Compiling.Lexing;
using EarleCode.Compiling.Parsing;
using EarleCode.Compiling.Parsing.Grammars;
using EarleCode.Utilities;

namespace EarleCode.Debug
{
    class Program
    {
        static void Main(string[] args)
        {
            var compiler = new EarleCompiler();

            var input = "#include \\foo\\bar;" +
                        "#include \\foo\\foo;" +
                        "random()" +
                        "{" +
                        "    return 4; // chosen by a fair dice roll\n" +
                        "}" +
                        "baz(a, b, c)" +
                        "{" +
                        "    result = a + b * c;" +
                        "    print(\"a + b * c equals \" + (result));" +
                        "    " +
                        "    randfunc = ::random;" +
                        "    " +
                        "    randval = who [[randfunc]]();" +
                        "    " +
                        "    print(\"Random value is \" + randval);" +
                        "}" +
                
                "";
            
            var tokens = compiler.Lexer.Tokenize(input).ToArray();

            var file = compiler.Parser.Parse(tokens);

            Console.WriteLine("Compilation completed.");
            Console.WriteLine("Functions: " + file.FunctionDeclarations.Count);
            foreach (var func in file.FunctionDeclarations)
                Console.WriteLine("- " + func.Name + "(" + string.Join(", ", func.Parameters ?? new string[0]) + "): " + func.Statements.Count + " statements");

            Console.WriteLine();
            
            Console.WriteLine("Includes: " + file.Includes.Count);
            
            foreach (var inc in file.Includes)
                Console.WriteLine("- " + inc.Path);

            Console.WriteLine();
        }
    }
}