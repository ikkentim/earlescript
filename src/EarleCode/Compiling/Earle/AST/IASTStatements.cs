using System.Collections.Generic;

namespace EarleCode.Compiling.Earle.AST
{
    public interface IASTStatements : IASTNode
    {
        IReadOnlyList<Statement> Statements { get; }
    }
}