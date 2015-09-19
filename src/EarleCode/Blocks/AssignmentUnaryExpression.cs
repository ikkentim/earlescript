using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EarleCode.Blocks
{
    public class AssignmentUnaryExpression : Block, IExpression
    {
        private readonly string _name;
        private readonly IExpression[] _indexers;//todo
        private readonly string _operatorToken;
        private readonly bool _isPostOperation;

        public AssignmentUnaryExpression(IScriptScope scriptScope, string name, IExpression[] indexers, string operatorToken, bool isPostOperation) : base(scriptScope)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (indexers == null) throw new ArgumentNullException(nameof(indexers));
            if (operatorToken == null) throw new ArgumentNullException(nameof(operatorToken));
            _name = name;
            _indexers = indexers;
            _operatorToken = operatorToken;
            _isPostOperation = isPostOperation;
        }

        private EarleValue SetVariable(string name, EarleValue value)
        {
            var variable = ResolveVariable(name) ?? AddVariable(name);
            variable.Set(value);

            return value;
        }

        #region Overrides of Block

        public override InvocationResult Invoke(Runtime runtime, IEarleContext context)
        {
            var variable = ResolveVariable(_name) ?? AddVariable(_name);

            var preValue = variable.Get();
            var postValue = new EarleValue(null);

            switch (_operatorToken)
            {
                case "--":
                    switch (preValue.Type)
                    {
                        case EarleValueType.Integer:
                            postValue = (int) preValue.Value - 1;
                            break;
                        case EarleValueType.Float:
                            postValue = (float)preValue.Value - 1;
                            break;
                    }
                    break;
                case "++":
                    switch (preValue.Type)
                    {
                        case EarleValueType.Integer:
                            postValue = (int)preValue.Value + 1;
                            break;
                        case EarleValueType.Float:
                            postValue = (float)preValue.Value + 1;
                            break;
                    }
                    break;
            }

            variable.Set(postValue);

            return new InvocationResult(InvocationState.None, _isPostOperation ? preValue : postValue);
        }

        public override InvocationResult Continue(Runtime runtime, IncompleteInvocationResult incompleteInvocationResult)
        {
            // only incomplete causes may be indexers which are not implemented yet
            throw new NotImplementedException();
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
            return _isPostOperation
                ? $"{_name}{string.Concat(_indexers.Select(i => $"[{i}]"))}{_operatorToken};"
                : $"{_operatorToken}{_name}{string.Concat(_indexers.Select(i => $"[{i}]"))}";
        }

        #endregion
    }
}
