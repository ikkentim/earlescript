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
        private static readonly Operator[] Operators =
        {
            // Number
            new Operator(VarType.Integer, "+", VarType.Integer,
                (l, r) => new ValueContainer(VarType.Integer, (int) l + (int) r)),
            new Operator(VarType.Integer, "-", VarType.Integer,
                (l, r) => new ValueContainer(VarType.Integer, (int) l - (int) r)),
            new Operator(VarType.Integer, "*", VarType.Integer,
                (l, r) => new ValueContainer(VarType.Integer, (int) l*(int) r)),
            new Operator(VarType.Integer, "/", VarType.Integer,
                (l, r) => new ValueContainer(VarType.Integer, (int) l/(int) r)),
            new Operator(VarType.Integer, "&&", VarType.Integer,
                (l, r) => new ValueContainer(VarType.Integer, (int) l != 0 && (int) r != 0 ? 1 : 0)),
            new Operator(VarType.Integer, "||", VarType.Integer,
                (l, r) => new ValueContainer(VarType.Integer, (int) l != 0 || (int) r != 0 ? 1 : 0)),
            new Operator(VarType.Integer, ">", VarType.Integer,
                (l, r) => new ValueContainer(VarType.Integer, (int) l > (int) r ? 1 : 0)),
            new Operator(VarType.Integer, "<", VarType.Integer,
                (l, r) => new ValueContainer(VarType.Integer, (int) l < (int) r ? 1 : 0)),
            new Operator(VarType.Integer, ">=", VarType.Integer,
                (l, r) => new ValueContainer(VarType.Integer, (int) l >= (int) r ? 1 : 0)),
            new Operator(VarType.Integer, "<=", VarType.Integer,
                (l, r) => new ValueContainer(VarType.Integer, (int) l <= (int) r ? 1 : 0)),
            new Operator(VarType.Integer, "==", VarType.Integer,
                (l, r) => new ValueContainer(VarType.Integer, (int) l == (int) r ? 1 : 0)),
            new Operator(VarType.Integer, "!=", VarType.Integer,
                (l, r) => new ValueContainer(VarType.Integer, (int) l != (int) r ? 1 : 0)),
            // String
            new Operator(VarType.String, "+", VarType.String,
                (l, r) => new ValueContainer(VarType.String, (string) l + (string) r)),
            new Operator(VarType.String, "+", VarType.Integer,
                (l, r) => new ValueContainer(VarType.String, (string) l + (int) r)),
            new Operator(VarType.Integer, "+", VarType.String,
                (l, r) => new ValueContainer(VarType.String, (int) l + (string) r))
        };

        public OperatorExpression(Block parent, Expression left, string op, Expression right) : base(parent)
        {
            if (left == null) throw new ArgumentNullException("left");
            if (op == null) throw new ArgumentNullException("op");
            if (right == null) throw new ArgumentNullException("right");

            Left = left;
            OP = op;
            Right = right;

            Left.Parent = this;
            Right.Parent = this;
        }

        public Expression Left { get; set; }
        public string OP { get; set; }
        public Expression Right { get; set; }

        #region Overrides of Block

        public override ValueContainer Run()
        {
            var leftValue = Left == null ? new ValueContainer(VarType.Null, null) : Left.Run();
            if (leftValue == null)
                return new ValueContainer(VarType.Null, null);
            var rightValue = Right == null ? new ValueContainer(VarType.Null, null) : Right.Run();

            var op =
                Operators.FirstOrDefault(
                    o => o.LeftType == leftValue.Type && o.RightType == rightValue.Type && o.OpString == OP);

            if (op == null)
                throw new RuntimeException(string.Format("Unsupported {1} operator `{0}` called", OP, leftValue.Type));

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
                    return new ValueContainer(VarType.Null, null);

                return _function(left, right);
            }
        }
    }
}