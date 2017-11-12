using System.Collections.Generic;

namespace EarleCode.Compiling.Parsing.AST
{
    public interface IAbstractSyntaxTreeInteriorNode : IAbstractSyntaxTreeNode
    {
        string Rule { get; }
        IEnumerable<IAbstractSyntaxTreeNode> Children { get; }
    }
}