using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EarleCode.Blocks
{
    public class StatementFor : Block
    {
        private readonly IBlock _assignmentExpression;
        private readonly IExpression _checkExpression;
        private readonly IBlock _incrementExpression;

        public StatementFor(IScriptScope scriptScope, IBlock assignmentExpression, IExpression checkExpression, IBlock incrementExpression) : base(scriptScope)
        {
            if (assignmentExpression == null) throw new ArgumentNullException(nameof(assignmentExpression));
            if (checkExpression == null) throw new ArgumentNullException(nameof(checkExpression));
            if (incrementExpression == null) throw new ArgumentNullException(nameof(incrementExpression));
            _assignmentExpression = assignmentExpression;
            _checkExpression = checkExpression;
            _incrementExpression = incrementExpression;
        }

        #region Overrides of Block

        public override InvocationResult Invoke(IEarleContext context)
        {
            _assignmentExpression?.Invoke(context);

            while (_checkExpression == null || _checkExpression.Invoke(context).ReturnValue.ToBoolean())
            {
                foreach (var block in Blocks)
                {
                    var result = block.Invoke(context);

                    if (result.State != InvocationState.None)
                        return result;
                }

                _incrementExpression?.Invoke(context);
            }

            return InvocationResult.Empty;
        }

        /// <summary>
        ///     Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        ///     A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            return $"for({_assignmentExpression};{_checkExpression};{_incrementExpression}) {{\n{base.ToString()}\n}}";
        }

        #endregion
    }
}
