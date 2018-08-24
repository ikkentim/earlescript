using EarleCode.Compiling.Lexing;

namespace EarleCode.Compiling.Earle.AST
{
    public class MultiplyExpression : BinaryExpression
    {
        public MultiplyExpression(FilePosition filePosition, Expression lhs, Expression rhs) : base(filePosition, lhs, rhs)
        {
        }
        
        protected override string OperatorSymbol => "*";
    }
}