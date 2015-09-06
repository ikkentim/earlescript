using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;

namespace EarleCode.Blocks
{
    public class AssignmentExpression : Block, IExpression
    {
        private readonly string _name;
        private readonly IExpression[] _indexers;
        private readonly IExpression _expression;

        public AssignmentExpression(IScriptScope scriptScope, string name, IExpression[] indexers, IExpression expression) : base(scriptScope)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (indexers == null) throw new ArgumentNullException(nameof(indexers));
            if (expression == null) throw new ArgumentNullException(nameof(expression));
            _name = name;
            _indexers = indexers;
            _expression = expression;
        }

        #region Overrides of Block

        public override InvocationResult Invoke(IEarleContext context)
        {
            var variable = ResolveVariable(_name) ?? AddVariable(_name);

            //todo : states
            var value = _expression.Invoke(context).ReturnValue;
            variable.Set(value);

            return new InvocationResult(InvocationState.None, value);
        }

        #endregion

        #region Overrides of Object

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            return $"{_name}{string.Concat(_indexers.Select(i => $"[{i}]"))} = {_expression}";
        }

        #endregion
    }
}
