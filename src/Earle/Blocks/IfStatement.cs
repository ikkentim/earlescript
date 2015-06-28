using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.XPath;
using Earle.Blocks.Expressions;
using Earle.Variables;

namespace Earle.Blocks
{
    public class IfStatement : Block
    {
        private readonly Expression _expression;

        public IfStatement(Block parent, Expression expression) : base(parent)
        {
            if (expression == null) throw new ArgumentNullException("expression");

            _expression = expression;

            expression.Parent = this;
        }

        #region Overrides of Block

        public override bool CanReturn
        {
            get { return true; }
        }

        public override ValueContainer Run()
        {
            var result = _expression.Run();

            if (result == null || result.Type == VarType.Null ||
                (result.Type == VarType.Integer && (int)result == 0) ||
                (result.Type == VarType.Float && (float)result == 0) ||
                (result.Type == VarType.Object && result.Value != null) ||
                (result.Type == VarType.String && string.IsNullOrEmpty(result.Value as string)) ||
                (result.Type == VarType.Target && result.Value == null))
                return null;

            return (Children.Select(block => new {block, value = block.Run()})
                .Where(a => a.value != null && a.block.CanReturn)
                .Select(a => a.value)).FirstOrDefault();
        }

        #endregion
    }
}
