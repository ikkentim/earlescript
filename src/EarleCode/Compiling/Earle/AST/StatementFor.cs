using System.Collections.Generic;
using System.Text;
using EarleCode.Compiling.Lexing;

namespace EarleCode.Compiling.Earle.AST
{
    public class StatementFor : Statement, IASTStatements
    {
        public StatementFor(FilePosition filePosition, IReadOnlyList<Expression> assignments,
            Expression condition, IReadOnlyList<Expression> increments, IReadOnlyList<Statement> statements) :
            base(filePosition)
        {
            Assignments = assignments;
            Condition = condition;
            Increments = increments;
            Statements = statements;
        }

        public IReadOnlyList<Expression> Assignments { get; }
        public Expression Condition { get; }
        public IReadOnlyList<Expression> Increments { get; }
        public IReadOnlyList<Statement> Statements { get; }
        
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("for (");
            sb.AppendList(Assignments);
            sb.Append("; ");
            if (Condition != null)
                sb.Append(Condition);
            
            sb.Append("; ");
            sb.AppendLine(")");
            sb.AppendLine("{");
            sb.AppendLines(Statements);
            sb.Append("} while (");
            sb.Append(Condition);
            sb.AppendLine(");");

            return sb.ToString();
        }
    }
}