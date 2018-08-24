using EarleCode.Compiling.Lexing;

namespace EarleCode.Compiling.Earle.AST
{
    public abstract class BinaryExpression : Expression
    {
        public BinaryExpression(FilePosition filePosition, Expression lhs, Expression rhs) : base(filePosition)
        {
            Lhs = lhs;
            Rhs = rhs;
        }

        public Expression Lhs { get; }
        public Expression Rhs { get; }
        protected abstract string OperatorSymbol { get; }
        
        public override string ToString()
        {
            return $"{Lhs} {OperatorSymbol} {Rhs}";
        }
    }
}