using EarleCode.Compiling.Lexing;

namespace EarleCode.Compiling.Earle.AST
{
    public abstract class FunctionIdentifier : ASTNode
    {
        protected FunctionIdentifier(FilePosition filePosition) : base(filePosition)
        {
        }
    }
}