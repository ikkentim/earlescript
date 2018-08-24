using System.Collections.Generic;
using System.Text;
using EarleCode.Compiling.Lexing;

namespace EarleCode.Compiling.Earle.AST
{
    public class StatementAssignments : Statement
    {
        public StatementAssignments(FilePosition filePosition, IReadOnlyList<AssignmentExpression> assignments) : base(filePosition)
        {
            Assignments = assignments;
        }

        public IReadOnlyList<AssignmentExpression> Assignments { get; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendList(Assignments);
            sb.Append(";");

            return sb.ToString();
        }
    }
}