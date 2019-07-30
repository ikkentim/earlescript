using EarleCode.Compiling.Lexing;

namespace EarleCode.Compiling.Earle.AST
{
    public class StatementWait : Statement
    {
        public StatementWait(FilePosition filePosition, Expression delay) : base(filePosition)
        {
            Delay = delay;
        }
        
        public Expression Delay { get; }

        public override string ToString()
        {
            return $"wait {Delay};";
        }
    }
}