using EarleCode.Compiling.Lexing;

namespace EarleCode.Compiling.Earle.AST
{
    public abstract class Expression : ASTNode
    {
        protected Expression(FilePosition filePosition) : base(filePosition)
        {
        }
    }
}