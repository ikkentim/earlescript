using EarleCode.Compiling.Lexing;

namespace EarleCode.Compiling.Earle.AST
{
    public class StatementContinue : Statement
    {
        public StatementContinue(FilePosition filePosition) : base(filePosition)
        {
        }
        
        public override string ToString()
        {
            return "continue;";
        }
    }
}