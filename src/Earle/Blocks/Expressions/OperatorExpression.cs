// Earle
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
using System.Linq;
using Earle.Variables;

namespace Earle.Blocks.Expressions
{
    public class OperatorExpression : Expression
    {
        #region Operators

        private static readonly Operator[] Operators =
        {
            // Integer-Integer
            new Operator(VarType.Integer, "*", VarType.Integer,
                (l, r) => new ValueContainer((int) l*(int) r)),
            new Operator(VarType.Integer, "/", VarType.Integer,
                (l, r) => new ValueContainer((int) l/(int) r)),
            new Operator(VarType.Integer, "%", VarType.Integer,
                (l, r) => new ValueContainer((int) l%(int) r)),
            new Operator(VarType.Integer, "+", VarType.Integer,
                (l, r) => new ValueContainer((int) l + (int) r)),
            new Operator(VarType.Integer, "-", VarType.Integer,
                (l, r) => new ValueContainer((int) l - (int) r)),
            new Operator(VarType.Integer, "<<", VarType.Integer,
                (l, r) => new ValueContainer((int) l << (int) r)),
            new Operator(VarType.Integer, ">>", VarType.Integer,
                (l, r) => new ValueContainer((int) l >> (int) r)),
            new Operator(VarType.Integer, "<", VarType.Integer,
                (l, r) => new ValueContainer((int) l < (int) r ? 1 : 0)),
            new Operator(VarType.Integer, ">", VarType.Integer,
                (l, r) => new ValueContainer((int) l > (int) r ? 1 : 0)),
            new Operator(VarType.Integer, "<=", VarType.Integer,
                (l, r) => new ValueContainer((int) l <= (int) r ? 1 : 0)),
            new Operator(VarType.Integer, ">=", VarType.Integer,
                (l, r) => new ValueContainer((int) l >= (int) r ? 1 : 0)),
            new Operator(VarType.Integer, "==", VarType.Integer,
                (l, r) => new ValueContainer((int) l == (int) r ? 1 : 0)),
            new Operator(VarType.Integer, "!=", VarType.Integer,
                (l, r) => new ValueContainer((int) l != (int) r ? 1 : 0)),
            new Operator(VarType.Integer, "&", VarType.Integer,
                (l, r) => new ValueContainer((int) l & (int) r)),
            new Operator(VarType.Integer, "|", VarType.Integer,
                (l, r) => new ValueContainer((int) l | (int) r)),
            new Operator(VarType.Integer, "&&", VarType.Integer,
                (l, r) => new ValueContainer(((int) l != 0) && ((int) r != 0) ? 1 : 0)),
            new Operator(VarType.Integer, "||", VarType.Integer,
                (l, r) => new ValueContainer(((int) l != 0) || ((int) r != 0) ? 1 : 0)),

            // Integer-Float
            new Operator(VarType.Integer, "*", VarType.Float,
                (l, r) => new ValueContainer((int) l*(float) r)),
            new Operator(VarType.Integer, "/", VarType.Float,
                (l, r) => new ValueContainer((int) l/(float) r)),
            new Operator(VarType.Integer, "+", VarType.Float,
                (l, r) => new ValueContainer((int) l + (float) r)),
            new Operator(VarType.Integer, "-", VarType.Float,
                (l, r) => new ValueContainer((int) l - (float) r)),
            new Operator(VarType.Integer, "<", VarType.Float,
                (l, r) => new ValueContainer((int) l < (float) r ? 1 : 0)),
            new Operator(VarType.Integer, ">", VarType.Float,
                (l, r) => new ValueContainer((int) l > (float) r ? 1 : 0)),
            new Operator(VarType.Integer, "<=", VarType.Float,
                (l, r) => new ValueContainer((int) l <= (float) r ? 1 : 0)),
            new Operator(VarType.Integer, ">=", VarType.Float,
                (l, r) => new ValueContainer((int) l >= (float) r ? 1 : 0)),
            new Operator(VarType.Integer, "==", VarType.Float,
                (l, r) => new ValueContainer((int) l == (float) r ? 1 : 0)),
            new Operator(VarType.Integer, "!=", VarType.Float,
                (l, r) => new ValueContainer((int) l != (float) r ? 1 : 0)),
             
            // Integer-String
            new Operator(VarType.Integer, "+", VarType.String,
                (l, r) => new ValueContainer((int) l + (string) r)),
  
            // Float-Integer
            new Operator(VarType.Float, "*", VarType.Integer,
                (l, r) => new ValueContainer((float) l*(int) r)),
            new Operator(VarType.Float, "/", VarType.Integer,
                (l, r) => new ValueContainer((float) l/(int) r)),
            new Operator(VarType.Float, "%", VarType.Integer,
                (l, r) => new ValueContainer((float) l%(int) r)),
            new Operator(VarType.Float, "+", VarType.Integer,
                (l, r) => new ValueContainer((float) l + (int) r)),
            new Operator(VarType.Float, "-", VarType.Integer,
                (l, r) => new ValueContainer((float) l - (int) r)),
            new Operator(VarType.Float, "<", VarType.Integer,
                (l, r) => new ValueContainer((float) l < (int) r ? 1 : 0)),
            new Operator(VarType.Float, ">", VarType.Integer,
                (l, r) => new ValueContainer((float) l > (int) r ? 1 : 0)),
            new Operator(VarType.Float, "<=", VarType.Integer,
                (l, r) => new ValueContainer((float) l <= (int) r ? 1 : 0)),
            new Operator(VarType.Float, ">=", VarType.Integer,
                (l, r) => new ValueContainer((float) l >= (int) r ? 1 : 0)),
            new Operator(VarType.Float, "==", VarType.Integer,
                (l, r) => new ValueContainer((float) l == (int) r ? 1 : 0)),
            new Operator(VarType.Float, "!=", VarType.Integer,
                (l, r) => new ValueContainer((float) l != (int) r ? 1 : 0)),

            // Float-Float
            new Operator(VarType.Float, "*", VarType.Float,
                (l, r) => new ValueContainer((float) l*(float) r)),
            new Operator(VarType.Float, "/", VarType.Float,
                (l, r) => new ValueContainer((float) l/(float) r)),
            new Operator(VarType.Float, "+", VarType.Float,
                (l, r) => new ValueContainer((float) l + (float) r)),
            new Operator(VarType.Float, "-", VarType.Float,
                (l, r) => new ValueContainer((float) l - (float) r)),
            new Operator(VarType.Float, "<", VarType.Float,
                (l, r) => new ValueContainer((float) l < (float) r ? 1 : 0)),
            new Operator(VarType.Float, ">", VarType.Float,
                (l, r) => new ValueContainer((float) l > (float) r ? 1 : 0)),
            new Operator(VarType.Float, "<=", VarType.Float,
                (l, r) => new ValueContainer((float) l <= (float) r ? 1 : 0)),
            new Operator(VarType.Float, ">=", VarType.Float,
                (l, r) => new ValueContainer((float) l >= (float) r ? 1 : 0)),
            new Operator(VarType.Float, "==", VarType.Float,
                (l, r) => new ValueContainer((float) l == (float) r ? 1 : 0)),
            new Operator(VarType.Float, "!=", VarType.Float,
                (l, r) => new ValueContainer((float) l != (float) r ? 1 : 0)),
             
            // Float-String
            new Operator(VarType.Float, "+", VarType.String,
                (l, r) => new ValueContainer((float) l + (string) r)),
  
            // String-X
            new Operator(VarType.String, "+", VarType.Integer,
                (l, r) => new ValueContainer((string) l + (int) r)),
            new Operator(VarType.String, "+", VarType.Float,
                (l, r) => new ValueContainer((string) l + (float) r)),
            new Operator(VarType.String, "+", VarType.String,
                (l, r) => new ValueContainer((string) l + (string) r)),
            new Operator(VarType.String, "==", VarType.String,
                (l, r) => new ValueContainer((string) l == (string) r ? 1 : 0)),
            new Operator(VarType.String, "!=", VarType.String,
                (l, r) => new ValueContainer((string) l != (string) r ? 1 : 0)),
            new Operator(VarType.String, "+", VarType.Null,
                (l, r) => new ValueContainer((string) l)),

            // Null-X
            new Operator(VarType.Null, "+", VarType.String,
                (l, r) => new ValueContainer((string) r)),
        };

        #endregion

        private readonly Operator[] _operators;

        public OperatorExpression(Block parent, Expression left, string op, Expression right) : base(parent)
        {
            if (left == null) throw new ArgumentNullException("left");
            if (op == null) throw new ArgumentNullException("op");
            if (right == null) throw new ArgumentNullException("right");

            Left = left;
            OP = op;
            Right = right;

            _operators = Operators.Where(o => o.OpString == op).ToArray();

            Left.Parent = this;
            Right.Parent = this;
        }

        public Expression Left { get; set; }
        public string OP { get; set; }
        public Expression Right { get; set; }

        #region Overrides of Block

        public override ValueContainer Run()
        {
            var leftValue = Left == null ? new ValueContainer() : Left.Run();
            if (leftValue == null)
                return new ValueContainer();
            var rightValue = Right == null ? new ValueContainer() : Right.Run();

            var op =
                _operators.FirstOrDefault(
                    o => o.LeftType == leftValue.Type && o.RightType == rightValue.Type);

            if (op == null)
                return new ValueContainer();

            return op.Calculate(leftValue, rightValue);
        }

        #endregion

        private class Operator
        {
            private readonly Func<ValueContainer, ValueContainer, ValueContainer> _function;

            public Operator(VarType leftType, string opString, VarType rightType,
                Func<ValueContainer, ValueContainer, ValueContainer> function)
            {
                if (function == null) throw new ArgumentNullException("function");
                _function = function;
                LeftType = leftType;
                OpString = opString;
                RightType = rightType;
            }

            public VarType LeftType { get; private set; }
            public VarType RightType { get; private set; }
            public string OpString { get; private set; }

            public ValueContainer Calculate(ValueContainer left, ValueContainer right)
            {
                if (left == null || left.Type == VarType.Null)
                    return new ValueContainer();

                return _function(left, right);
            }
        }
    }
}