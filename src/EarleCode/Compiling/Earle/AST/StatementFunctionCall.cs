using EarleCode.Compiling.Lexing;

namespace EarleCode.Compiling.Earle.AST
{
    public class StatementFunctionCall : Statement
    {
        public StatementFunctionCall(FilePosition filePosition, FunctionCall function) : base(filePosition)
        {
            Function = function;
        }

        public FunctionCall Function { get; }

        public override string ToString()
        {
            return $"{Function};";
        }
    }
}