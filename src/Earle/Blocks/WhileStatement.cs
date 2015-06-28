using System;
using System.Linq;
using Earle.Blocks.Expressions;
using Earle.Variables;

namespace Earle.Blocks
{
    public class WhileStatement : Block
    {
        private readonly Expression _expression;

        public WhileStatement(Block parent, Expression expression)
            : base(parent, true)
        {
            if (expression == null) throw new ArgumentNullException("expression");

            _expression = expression;

            expression.Parent = this;
        }

        #region Overrides of Block

        public override ValueContainer Run()
        {
            while (true)
            {
                var result = _expression.Run();

                if (result == null || result.Type == VarType.Null ||
                    (result.Type == VarType.Integer && (int) result == 0) ||
                    (result.Type == VarType.Float && (float) result == 0) ||
                    (result.Type == VarType.Object && result.Value != null) ||
                    (result.Type == VarType.String && string.IsNullOrEmpty(result.Value as string)) ||
                    (result.Type == VarType.Target && result.Value == null))
                    return null;

                foreach (
                    var value in
                        Children.Select(block => new {block, value = block.Run()})
                            .Where(a => a.value != null && a.block.CanReturn)
                            .Select(a => a.value))
                {
                    return value;
                }
            }

            #endregion
        }
    }
}