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
        private readonly IExpression[] _indexers;//todo
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

        private EarleValue SetVariable(string name, EarleValue value)
        {
            var variable = ResolveVariable(name) ?? AddVariable(name);
            variable.Set(value);

            return value;
        }

        #region Overrides of Block

        public override InvocationResult Invoke(IEarleContext context)
        {
            var result = _expression.Invoke(context);

            return result.State == InvocationState.Incomplete
                ? new InvocationResult(result.Result)
                : new InvocationResult(InvocationState.None, SetVariable(_name, result.ReturnValue));
        }

        public override InvocationResult Continue(IncompleteInvocationResult incompleteInvocationResult)
        {
            if(incompleteInvocationResult?.InnerResult == null)
                throw new Exception();

            var result = _expression.Continue(incompleteInvocationResult.InnerResult);

            return result.State == InvocationState.Incomplete
                ? new InvocationResult(result.Result)
                : new InvocationResult(InvocationState.None, SetVariable(_name, result.ReturnValue));
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
            return $"{_name}{string.Concat(_indexers.Select(i => $"[{i}]"))} = {_expression};";
        }

        #endregion
    }
}
