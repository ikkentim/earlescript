using System.Collections.Generic;

namespace EarleCode.Compiling.Parsing.AST
{
    public class AbstractSyntaxTreeInteriorNode : IAbstractSyntaxTreeInteriorNode
    {
        public AbstractSyntaxTreeInteriorNode(string rule, IEnumerable<IAbstractSyntaxTreeNode> children)
        {
            Rule = rule;
            Children = children;
        }

        #region Implementation of IAbstractSyntaxTreeInteriorNode

        public string Rule { get; }
        public IEnumerable<IAbstractSyntaxTreeNode> Children { get; }

        #endregion
    }
}