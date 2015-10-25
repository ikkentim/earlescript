using System;
using EarleCode.Blocks.Expressions;

namespace EarleCode.Blocks.Statements
{
    public class StatementFor : ScopeBlock
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

        public override InvocationResult Invoke(Runtime runtime, IEarleContext context)
        {
            Variables.Clear();

            if (_assignmentExpression != null)
            {
                var result = _assignmentExpression.Invoke(runtime, context);

                if (result.State == InvocationState.Incomplete)
                    return new InvocationResult(new IncompleteInvocationResult("for 0", context, result.IncompleteResult) {Stage=0, Variables = Variables.Clone() });
            }

            while (true)
            {
                if (_checkExpression != null)
                {
                    var result = _checkExpression.Invoke(runtime, context);
                    if (result.State == InvocationState.Incomplete)
                        return new InvocationResult(new IncompleteInvocationResult("for 1", context, result.IncompleteResult) { Stage = 1, Variables = Variables.Clone() });

                    if (!result.ReturnValue.ToBoolean())
                        break;
                }

                {
                    var result = InvokeBlocks(runtime, context);
                    if (result.State == InvocationState.Incomplete)
                        return new InvocationResult(new IncompleteInvocationResult("for 2", context, result.IncompleteResult) { Stage = 2, Variables = Variables.Clone() });
                }

                if (_incrementExpression != null)
                {
                    var result = _incrementExpression.Invoke(runtime, context);
                    if (result.State == InvocationState.Incomplete)
                        return new InvocationResult(new IncompleteInvocationResult("for 3", context, result.IncompleteResult) { Stage = 3, Variables = Variables.Clone() });
                }
            }

            return InvocationResult.Empty;
        }

        public override InvocationResult Continue(Runtime runtime, IncompleteInvocationResult incompleteInvocationResult)
        {
            Variables = incompleteInvocationResult.Variables;

            if (_assignmentExpression != null && incompleteInvocationResult.Stage == 0)
            {
                var result = _assignmentExpression.Continue(runtime, incompleteInvocationResult);

                if (result.State == InvocationState.Incomplete)
                    return
                        new InvocationResult(new IncompleteInvocationResult("for c0", incompleteInvocationResult.Context,
                            result.IncompleteResult) {Stage = 0, Variables = Variables.Clone()});
            }

            var continueStage = incompleteInvocationResult.Stage;
            while (true)
            {
                if (_checkExpression != null && continueStage <= 1)
                {
                    var result = continueStage == 1
                        ? _checkExpression.Continue(runtime, incompleteInvocationResult.InnerResult)
                        : _checkExpression.Invoke(runtime, incompleteInvocationResult.Context);

                    if (result.State == InvocationState.Incomplete)
                        return
                            new InvocationResult(new IncompleteInvocationResult("for c1", incompleteInvocationResult.Context,
                                result.IncompleteResult)
                            { Stage = 1, Variables = Variables.Clone() });

                    if (!result.ReturnValue.ToBoolean())
                        break;
                }

                if(continueStage <= 2)
                {
                    var result = continueStage == 2
                        ? ContinueBlocks(runtime, incompleteInvocationResult.InnerResult)
                        : InvokeBlocks(runtime, incompleteInvocationResult.Context);

                    if (result.State == InvocationState.Incomplete)
                        return
                            new InvocationResult(new IncompleteInvocationResult("for c2", incompleteInvocationResult.Context,
                                result.IncompleteResult)
                            { Stage = 2, Variables = Variables.Clone() });
                }

                if (_incrementExpression != null && continueStage <= 3)
                {
                    var result = continueStage == 3
                        ? _incrementExpression.Continue(runtime, incompleteInvocationResult.InnerResult)
                        : _incrementExpression.Invoke(runtime, incompleteInvocationResult.Context);

                    if (result.State == InvocationState.Incomplete)
                        return
                            new InvocationResult(new IncompleteInvocationResult("for c3", incompleteInvocationResult.Context,
                                result.IncompleteResult)
                            { Stage = 3, Variables = Variables.Clone() });
                }

                continueStage = 0;
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
