using EarleCode.Compiling.Lexing;

namespace EarleCode.Compiling.Earle.AST
{
    public class ImplicitFunctionIdentifier : FunctionIdentifier
    {
        public ImplicitFunctionIdentifier(FilePosition filePosition, string name) : base(filePosition)
        {
            Name = name;
        }

        public string Name { get; }

        public override string ToString()
        {
            return Name;
        }
    }
}