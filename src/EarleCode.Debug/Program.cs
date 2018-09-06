using System;
using System.IO;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
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
            var interp = new EarleInterpreter((name) =>
            {
                switch (name)
                {
                     case "\\main":
                         return File.ReadAllText("scripts/main.earle");
                     default:
                         return null;
                }
            });

            var main = interp["\\main"]["main"];

            var ok = interp.Invoke(main, out var res);

            Console.WriteLine($"OK: {ok}, result: {res}");
        }
        
        static void Main2(string[] args)
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

            var sw = new Stopwatch();

            sw.Start();
            var file = compiler.Compile(input, "testfile");
            sw.Stop();
            Console.WriteLine("Compilation completed. Took " + sw.Elapsed);

            Console.WriteLine("Functions: " + file.FunctionDeclarations.Count);
            foreach (var func in file.FunctionDeclarations)
                Console.WriteLine("- " + func.Name + "(" + string.Join(", ", func.Parameters ?? new string[0]) + "): " +
                                  func.Statements.Count + " statements");

            Console.WriteLine();

            Console.WriteLine("Includes: " + file.Includes.Count);

            foreach (var inc in file.Includes)
                Console.WriteLine("- " + inc.Path);

            Console.WriteLine();
        }
    }
}
