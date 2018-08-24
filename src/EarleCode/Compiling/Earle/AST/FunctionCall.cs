using System.Collections.Generic;
using System.Text;
using EarleCode.Compiling.Lexing;

namespace EarleCode.Compiling.Earle.AST
{
    public class FunctionCall : Expression
    {
        public FunctionCall(FilePosition filePosition, FunctionIdentifier functionIdentifier, IReadOnlyList<Expression> arguments, bool isThreaded, Expression target) : base(filePosition)
        {
            FunctionIdentifier = functionIdentifier;
            Arguments = arguments;
            IsThreaded = isThreaded;
            Target = target;
        }

        public FunctionIdentifier FunctionIdentifier { get; }
        public IReadOnlyList<Expression> Arguments { get; }
        public bool IsThreaded { get; }
        public Expression Target { get; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            if (Target != null)
            {
                sb.Append(sb);
                sb.Append(" ");
            }

            if (IsThreaded)
            {
                sb.Append("thread ");
            }

            sb.Append(FunctionIdentifier);
            sb.Append("(");
            sb.AppendList(Arguments);
            sb.Append(")");

            return sb.ToString();
        }
    }
}