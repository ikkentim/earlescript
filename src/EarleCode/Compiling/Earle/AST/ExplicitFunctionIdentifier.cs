using EarleCode.Compiling.Lexing;

namespace EarleCode.Compiling.Earle.AST
{
    public class ExplicitFunctionIdentifier : FunctionIdentifier
    {
        public ExplicitFunctionIdentifier(FilePosition filePosition, string path, string name) : base(filePosition)
        {
            Path = path;
            Name = name;
        }

        public string Path { get; }
        public string Name { get; }

        public override string ToString()
        {
            return Path == null ? $"::{Name}" : $"{Path}::{Name}";
        }
    }
}