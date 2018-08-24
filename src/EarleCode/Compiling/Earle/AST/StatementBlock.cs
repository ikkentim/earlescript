using System.Collections.Generic;
using System.Text;
using EarleCode.Compiling.Lexing;

namespace EarleCode.Compiling.Earle.AST
{
    public class StatementBlock : Statement, IASTStatements
    {
        public StatementBlock(FilePosition filePosition, IReadOnlyList<Statement> statements) : base(filePosition)
        {
            Statements = statements;
        }

        public IReadOnlyList<Statement> Statements { get; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLines(Statements);
            
            return sb.ToString();
        }
    }
}