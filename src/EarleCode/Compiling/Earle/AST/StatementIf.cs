using System.Collections.Generic;
using System.Text;
using EarleCode.Compiling.Lexing;

namespace EarleCode.Compiling.Earle.AST
{
    public class StatementIf : Statement, IASTStatements
    {
        public StatementIf(FilePosition filePosition, Expression condition, IReadOnlyList<Statement> statements, IReadOnlyList<Statement> elseStatements) : base(filePosition)
        {
            Condition = condition;
            Statements = statements;
            ElseStatements = elseStatements;
        }

        public Expression Condition { get; }
        public IReadOnlyList<Statement> Statements { get; }
        public IReadOnlyList<Statement> ElseStatements { get; }
        
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("if (");
            sb.Append(Condition);
            sb.AppendLine(")");
            sb.AppendLine("{");
            sb.AppendLines(Statements);
            sb.AppendLine("}");
            sb.AppendLine("else");
            sb.AppendLine("{");
            sb.AppendLines(ElseStatements);
            sb.AppendLine("}");

            return sb.ToString();
        }
    }
}