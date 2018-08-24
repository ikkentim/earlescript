using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using EarleCode.Compiling;
using EarleCode.Compiling.Earle.AST;
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
                         return "main()" +
                                "{" +
                                "foo();" +
                                "}";
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

    public delegate string EarleFileLoader(string path);

    public class EarleFunction
    {
        public FunctionDeclaration Node { get; }

        public EarleFile File { get; }
        public EarleFunction(EarleFile file, FunctionDeclaration node)
        {
            Node = node ?? throw new ArgumentNullException(nameof(node));
        }
    }

    public class EarleFile
    {
        private readonly Dictionary<string, EarleFunction> _functions;

        public EarleFile(ProgramFile node)
        {
            Node = node ?? throw new ArgumentNullException(nameof(node));

            if (node.FunctionDeclarations == null)
                return;

            _functions = new Dictionary<string, EarleFunction>();
            foreach (var function in node.FunctionDeclarations)
                _functions[function.Name] = new EarleFunction(this, function);
        }

        public ProgramFile Node { get; }

        public EarleFunction this[string name] => GetFunction(name);

        private EarleFunction GetFunction(string name)
        {
            if (_functions == null)
                return null;

            _functions.TryGetValue(name, out var result);
            return result;
        }
    }

    public struct EarleValue
    {
        private int _intValue;
        private float _floatValue;
        private string _stringValue;
        private EarleFunction _functionValue;
        private EarleValueType _type;

        public static readonly EarleValue Null = new EarleValue();
        
        public EarleValue(int value)
        {
            _intValue = value;
            _floatValue = 0;
            _stringValue = null;
            _functionValue = null;
            _type = EarleValueType.NumberInt;
        }

        public EarleValue(float value)
        {
            _intValue = 0;
            _floatValue = value;
            _stringValue = null;
            _functionValue = null;
            _type = EarleValueType.NumberFloat;
        }

        public EarleValue(string value)
        {
            _intValue = 0;
            _floatValue = 0;
            _stringValue = value;
            _functionValue = null;
            _type = EarleValueType.String;
        }

        public EarleValue(EarleFunction value)
        {
            _intValue = 0;
            _floatValue = 0;
            _stringValue = null;
            _functionValue = value;
            _type = EarleValueType.FunctionPointer;
        }
    }

    [Flags]
    public enum EarleValueType
    {
        NumberInt     = 0b0000000001,
        NumberFloat   = 0b0000000010,
        String        = 0b0000000100,
        FunctionPointer=0b0000001000,
        Number        = 0b0000000011,
        Struct        = 0b0000010000, // todo
        Vector2       = 0b0000100000, // todo
        Vector3       = 0b0001000000, // todo
        Vector4       = 0b0010000000, // todo
        Array         = 0b0100000000, // todo
        Null          = 0b0000000000
    }

    public class EarleInterpreter
    {
        private readonly EarleFileLoader _loader;
        private readonly Dictionary<string, EarleFile> _files = new Dictionary<string, EarleFile>();

        public EarleInterpreter(EarleFileLoader loader)
        {
            _loader = loader ?? throw new ArgumentNullException(nameof(loader));
            Compiler = new EarleCompiler();
        }

        public EarleCompiler Compiler { get; }

        public EarleFile this[string path] => GetFile(path);

        public EarleFile LoadFile(string path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));

            var input = _loader(path);

            var file = Compiler.Compile(input);

            return new EarleFile(file);
        }

        private EarleFile GetFile(string path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));

            if (_files.TryGetValue(path, out var file))
                return file;

            file = LoadFile(path);
            _files[path] = file;

            return file;
        }

        public bool Invoke(EarleFunction function, out EarleValue result, params EarleValue[] args)
        {
            var frm = new InterpreterFrameExecutor();

            var scope = new Scope();

            if(function.Node.Parameters != null && args != null)
            for (var i = 0; i < function.Node.Parameters.Count; i++)
            {
                scope.Variables[function.Node.Parameters[i]] = args.Length <= i ? EarleValue.Null : args[i];
            }
            frm.Function = function;
            frm.Scopes.Push(scope);
            frm.Interpreter = this;

            var res = frm.Run();
            if (res != null)
            {
                result = res.Value;
                return true;
            }

            result = EarleValue.Null;
            
            return false;
        }
    }

    public interface IFrameExecutor
    {
        EarleValue? Run();
    }
    /// <summary>
    /// Executor of a "function"
    /// </summary>
    public class InterpreterFrameExecutor : IFrameExecutor
    {
        public EarleValue Target;
        public List<int> Path = new List<int>();
        public Stack<EarleValue> Stack = new Stack<EarleValue>();
        public Stack<Scope> Scopes = new Stack<Scope>();
        public IFrameExecutor SubFrame;

        public EarleFunction Function;
        public EarleInterpreter Interpreter;
        
        public EarleValue?  Run()
        {
            if (SubFrame != null)
            {
                var subResult = SubFrame.Run();
                if (subResult != null)
                {
                    Stack.Push(subResult.Value);
                    SubFrame = null;
                }
                else
                    return null;
            }

            if (Run(Function.Node, 0))
            {
                // TODO: Stack.count should never be 0 at this point
                return Stack.Count == 0 ? EarleValue.Null : Stack.Pop();
            }
            
            // TODO: If subframe is set, try it once

            return null;
        }

        private bool Run(IASTNode node, int pathIndex)
        {
            if (Path.Count == pathIndex)
            {
                // new start
                Path.Add(0);
            }
            else if(Path.Count < pathIndex)
            {
                // error
                throw new Exception("Unexpected frame path jump");
            }

            var length = GetLength(node);
            var index = Path[pathIndex];
            while(index < length)
            {
                if (Execute(node, index, pathIndex))
                {
                    index++;
                }
                else
                {
                    return false;
                }
            }

            Path[pathIndex] = index;

            
            // done
            if(pathIndex != Path.Count - 1) 
                throw new Exception("Frame path corrupted");
            
            Path.RemoveAt(pathIndex);

            return true;
        }

        private bool Execute(IASTNode node, int index, int pathIndex)
        {
            // return true on early finish (before index hits length)
            switch (node)
            {
                case FunctionDeclaration decl:
                    if (!Run(decl.Statements[index], pathIndex + 1)) return false;
                    break;
                case StatementBlock block:
                    if (!Run(block.Statements[index], pathIndex + 1)) return false;
                    break;
                case StatementFunctionCall stcall:
                    if (!Run(stcall.Function, pathIndex + 1)) return false;
                    Stack.Pop();
                    return true;
                case FunctionCall call:
                    if (call.Arguments != null && index < call.Arguments.Count)
                    {
                        Run(call.Arguments[index], pathIndex + 1);
                    }
                    else
                    {
                        EarleFunction callingFunction = null;
                        switch (call.FunctionIdentifier)
                        {
                            case ImplicitFunctionIdentifier impl:
                                callingFunction = Function.File[impl.Name];
                                break;
                            default:
                                throw new NotImplementedException();
                        }
                        
                        var frm = new InterpreterFrameExecutor();
                        var scope = new Scope();
                        for (var i = 0; i < callingFunction.Node.Parameters.Count; i++)
                        {
                            scope.Variables[callingFunction.Node.Parameters[i]] = args.Length <= i ? EarleValue.Null : args[i];
                        }
                        frm.Function = callingFunction;
                        frm.Scopes.Push(scope);
                    }
                default:
                    throw new NotImplementedException();
            }

            return false;
        }
        
        private int GetLength(IASTNode node)
        {
            switch (node)
            {
                case FunctionDeclaration decl:
                    return decl.Statements?.Count ?? 0;
                case StatementBlock block:
                    return block.Statements?.Count ?? 0;
                case StatementFunctionCall _:
                    return 1;
                case FunctionCall call:
                    return (call.Arguments?.Count ?? 0) + 1;
                default:
                    throw new NotImplementedException();
            }
        }
    }
    
    public class Scope
    {
        public Dictionary<string, EarleValue> Variables = new Dictionary<string, EarleValue>();
    }
}