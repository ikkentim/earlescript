// EarleCode
// Copyright 2015 Tim Potze
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;

namespace EarleCode.Blocks
{
    public class OperatorExpression : Block, IExpression
    {
        public IExpression LeftExpression { get; }
        public string OperatorToken { get; }
        public IExpression RightExpression { get; }

        public OperatorExpression(IScriptScope scriptScope, IExpression leftExpression, string operatorToken,
            IExpression rightExpression) : base(scriptScope)
        {
            if (leftExpression == null) throw new ArgumentNullException(nameof(leftExpression));
            if (operatorToken == null) throw new ArgumentNullException(nameof(operatorToken));
            if (rightExpression == null) throw new ArgumentNullException(nameof(rightExpression));
            LeftExpression = leftExpression;
            OperatorToken = operatorToken;
            RightExpression = rightExpression;
        }

        #region Overrides of Block

        public override InvocationResult Invoke(IEarleContext context)
        {
            // TODO states...
            // TODO is messy
            // TODO comparators are messy and badly casted to float
            EarleValue leftValue, rightValue;
            switch (OperatorToken)
            {
                case "+":
                    leftValue = LeftExpression.Invoke(context).ReturnValue;
                    rightValue = RightExpression.Invoke(context).ReturnValue;
                    return new InvocationResult(InvocationState.None, EarleValue.Add(leftValue, rightValue));
                case "-":
                    leftValue = LeftExpression.Invoke(context).ReturnValue;
                    rightValue = RightExpression.Invoke(context).ReturnValue;
                    return new InvocationResult(InvocationState.None, EarleValue.Subtract(leftValue, rightValue));
                case "&&":
                    leftValue = LeftExpression.Invoke(context).ReturnValue;
                    if(!leftValue.ToBoolean())
                        return new InvocationResult(InvocationState.None, 0);

                    rightValue = RightExpression.Invoke(context).ReturnValue;
                    return !rightValue.ToBoolean()
                        ? new InvocationResult(InvocationState.None, 0)
                        : new InvocationResult(InvocationState.None, 1);
                case "||":
                    leftValue = LeftExpression.Invoke(context).ReturnValue;
                    if (leftValue.ToBoolean())
                        return new InvocationResult(InvocationState.None, 1);

                    rightValue = RightExpression.Invoke(context).ReturnValue;
                    return rightValue.ToBoolean()
                        ? new InvocationResult(InvocationState.None, 1)
                        : new InvocationResult(InvocationState.None, 0);
                case "<":
                    leftValue = LeftExpression.Invoke(context).ReturnValue;
                    rightValue = RightExpression.Invoke(context).ReturnValue;
                    return new InvocationResult(InvocationState.None,
                        ((float)leftValue.CastTo(EarleValueType.Float).Value <
                         (float)rightValue.CastTo(EarleValueType.Float).Value)
                            ? 1
                            : 0);
                case ">":
                    leftValue = LeftExpression.Invoke(context).ReturnValue;
                    rightValue = RightExpression.Invoke(context).ReturnValue;
                    return new InvocationResult(InvocationState.None,
                        ((float)leftValue.CastTo(EarleValueType.Float).Value >
                         (float)rightValue.CastTo(EarleValueType.Float).Value)
                            ? 1
                            : 0);
                case "<=":
                    leftValue = LeftExpression.Invoke(context).ReturnValue;
                    rightValue = RightExpression.Invoke(context).ReturnValue;
                    return new InvocationResult(InvocationState.None,
                        ((float)leftValue.CastTo(EarleValueType.Float).Value <=
                         (float)rightValue.CastTo(EarleValueType.Float).Value)
                            ? 1
                            : 0);
                case ">=":
                    leftValue = LeftExpression.Invoke(context).ReturnValue;
                    rightValue = RightExpression.Invoke(context).ReturnValue;
                    return new InvocationResult(InvocationState.None,
                        ((float)leftValue.CastTo(EarleValueType.Float).Value >=
                         (float)rightValue.CastTo(EarleValueType.Float).Value)
                            ? 1
                            : 0);
                default:
                    throw new NotImplementedException();
            }
        }

        public override InvocationResult Continue(IncompleteInvocationResult incompleteInvocationResult)
        {
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
            return $"{LeftExpression} {OperatorToken} {RightExpression}";
        }

        #endregion
    }
}