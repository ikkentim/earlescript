using EarleCode.Compiling.Lexing;

namespace EarleCode.Compiling.Earle.AST
{
    public interface IASTNode
    {
        FilePosition FilePosition { get; }
    }
}