using System;
using System.Collections.Generic;
using System.Text;
using EarleCode.Compiling.Lexing;

namespace EarleCode.Compiling.Earle.AST
{
    public class FunctionDeclaration : ASTNode, IASTStatements
    {
        public FunctionDeclaration(FilePosition filePosition, string name, IReadOnlyList<string> parameters, IReadOnlyList<Statement> statements) : base(filePosition)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Parameters = parameters;
            Statements = statements ?? throw new ArgumentNullException(nameof(statements));
        }

        public string Name { get; }
        public IReadOnlyList<string> Parameters { get; }
        public IReadOnlyList<Statement> Statements { get; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(Name);
            sb.Append(" (");
            sb.AppendList(Parameters);
            sb.AppendLine(")");
            sb.AppendLine("{");
            sb.AppendLines(Statements);
            sb.AppendLine("}");

            return sb.ToString();
        }
    }
}