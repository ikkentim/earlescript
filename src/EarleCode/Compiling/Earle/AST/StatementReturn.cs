using System.Text;
using EarleCode.Compiling.Lexing;

namespace EarleCode.Compiling.Earle.AST
{
    public class StatementReturn : Statement
    {
        public StatementReturn(FilePosition filePosition, Expression expression) : base(filePosition)
        {
            Expression = expression;
        }

        public Expression Expression { get; }
        
        public override string ToString()
        {
            if (Expression == null)
                return "return;";
            
            var sb = new StringBuilder();
            sb.Append("return ");
            sb.Append(Expression);
            sb.Append(";");

            return sb.ToString();
        }
    }
}