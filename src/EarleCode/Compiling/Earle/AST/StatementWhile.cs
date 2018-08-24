using System.Collections.Generic;
using System.Text;
using EarleCode.Compiling.Lexing;

namespace EarleCode.Compiling.Earle.AST
{
    public class StatementWhile : Statement, IASTStatements
    {
        public StatementWhile(FilePosition filePosition, Expression condition, IReadOnlyList<Statement> statements) : base(filePosition)
        {
            Condition = condition;
            Statements = statements;
        }

        public Expression Condition { get; }
        public IReadOnlyList<Statement> Statements { get; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("while (");
            sb.Append(Condition);
            sb.AppendLine(")");
            sb.AppendLine("{");
            sb.AppendLines(Statements);
            sb.AppendLine("}");

            return sb.ToString();
        }
    }
}