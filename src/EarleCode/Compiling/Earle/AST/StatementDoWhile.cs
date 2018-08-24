using System.Collections.Generic;
using System.Text;
using EarleCode.Compiling.Lexing;

namespace EarleCode.Compiling.Earle.AST
{
    public class StatementDoWhile : Statement, IASTStatements
    {
        public StatementDoWhile(FilePosition filePosition, Expression condition, IReadOnlyList<Statement> statements) : base(filePosition)
        {
            Condition = condition;
            Statements = statements;
        }

        public Expression Condition { get; }
        public IReadOnlyList<Statement> Statements { get; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("do");
            sb.AppendLine("{");
            sb.AppendLines(Statements);
            sb.Append("} while (");
            sb.Append(Condition);
            sb.AppendLine(");");

            return sb.ToString();
        }
    }
}