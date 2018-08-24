using EarleCode.Compiling.Lexing;

namespace EarleCode.Compiling.Earle.AST
{
    public class UnboxedFunctionIdentifier : FunctionIdentifier
    {
        public UnboxedFunctionIdentifier(FilePosition filePosition, Expression expression) : base(filePosition)
        {
            Expression = expression;
        }

        public Expression Expression { get; }

        public override string ToString()
        {
            return $"[[{Expression}]]";
        }
    }
}