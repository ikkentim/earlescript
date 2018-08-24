using EarleCode.Compiling.Lexing;

namespace EarleCode.Compiling.Earle.AST
{
    public abstract class Statement : ASTNode
    {
        protected Statement(FilePosition filePosition) : base(filePosition)
        {
        }
    }
}