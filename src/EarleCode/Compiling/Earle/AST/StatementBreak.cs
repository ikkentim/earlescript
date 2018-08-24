using EarleCode.Compiling.Lexing;

namespace EarleCode.Compiling.Earle.AST
{
    public class StatementBreak : Statement
    {
        public StatementBreak(FilePosition filePosition) : base(filePosition)
        {
        }
        
        public override string ToString()
        {
            return "break;";
        }
    }
}