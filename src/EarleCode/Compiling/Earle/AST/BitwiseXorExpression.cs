using EarleCode.Compiling.Lexing;

namespace EarleCode.Compiling.Earle.AST
{
    public class BitwiseXorExpression : BinaryExpression
    {
        public BitwiseXorExpression(FilePosition filePosition, Expression lhs, Expression rhs) : base(filePosition, lhs, rhs)
        {
        }
        
        protected override string OperatorSymbol => "^";
    }
}