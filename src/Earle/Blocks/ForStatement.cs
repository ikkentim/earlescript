using System.Collections.Generic;
using System.Linq;
using Earle.Blocks.Expressions;
using Earle.Variables;

namespace Earle.Blocks
{
    public class ForStatement : Block
    {
        private readonly Assignment _initialisation;
        private readonly Expression _condition;
        private readonly Assignment _incrementation;
        private readonly Dictionary<string, ValueContainer> _variables = new Dictionary<string, ValueContainer>(); 

        public ForStatement(Block parent, Assignment initialisation, Expression condition, Assignment incrementation)
            : base(parent, true)
        {
            _initialisation = initialisation;
            _condition = condition;
            _incrementation = incrementation;

            if (initialisation != null)
                initialisation.Parent = this;

            if (condition != null)
                condition.Parent = this;

            if (incrementation != null)
                incrementation.Parent = this;
        }

        #region Overrides of Block

        public override void AddVariable(string name, ValueContainer value)
        {
            _variables[name] = value;
        }

        public override ValueContainer ResolveVariable(string name)
        {
            return base.ResolveVariable(name) ?? (_variables.ContainsKey(name) ? _variables[name] : null);
        }

        public override ValueContainer Run()
        {
            if (_initialisation != null)
                _initialisation.Run();

            while (true)
            {
                if (_condition != null)
                {
                    var result = _condition.Run();

                    if (result == null || result.Type == VarType.Null ||
                        (result.Type == VarType.Integer && (int) result == 0) ||
                        (result.Type == VarType.Float && (float) result == 0) ||
                        (result.Type == VarType.Object && result.Value != null) ||
                        (result.Type == VarType.String && string.IsNullOrEmpty(result.Value as string)) ||
                        (result.Type == VarType.Target && result.Value == null))
                        return null;
                }

                foreach (
                    var value in
                        Children.Select(block => new { block, value = block.Run() })
                            .Where(a => a.value != null && a.block.CanReturn)
                            .Select(a => a.value))
                {
                    return value;
                }

                if (_incrementation != null)
                    _incrementation.Run();
            }

            #endregion
        }
    }
}