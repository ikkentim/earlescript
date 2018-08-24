using EarleCode.Compiling.Lexing;

namespace EarleCode.Compiling.Earle.AST
{
    public class ASTNode : IASTNode
    {
        public ASTNode(FilePosition filePosition)
        {
            FilePosition = filePosition;
        }

        public FilePosition FilePosition { get; }
    }
}