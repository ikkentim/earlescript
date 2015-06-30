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
    public class UnaryOperatorExpression : Expression
    {
        private static readonly Operator[] Operators =
        {
            // Number
            new Operator("+", VarType.Integer, v => v.Clone()),
            new Operator("-", VarType.Integer, v => new ValueContainer(-(int) v)),
            new Operator("!", VarType.Integer, v => new ValueContainer((int) v == 0 ? 1 : 0))
        };

        public UnaryOperatorExpression(Block parent) : base(parent)
        {
        }

        public UnaryOperatorExpression(Block parent, string op, Expression expression)
            : base(parent)
        {
            if (op == null) throw new ArgumentNullException("op");
            if (expression == null) throw new ArgumentNullException("expression");
            OP = op;
            Value = expression;

            expression.Parent = this;
        }

        public string OP { get; set; }
        public Expression Value { get; set; }

        #region Overrides of Block

        public override ValueContainer Run()
        {
            var value = Value == null ? new ValueContainer() : Value.Run();
            if (value == null)
                return new ValueContainer();

            var op = Operators.FirstOrDefault(o => o.Type == value.Type && o.OpString == OP);

            if (op == null)
                return new ValueContainer();

            return op.Calculate(value);
        }

        #endregion

        private class Operator
        {
            private readonly Func<ValueContainer, ValueContainer> _function;

            public Operator(string opString, VarType type,
                Func<ValueContainer, ValueContainer> function)
            {
                if (function == null) throw new ArgumentNullException("function");
                _function = function;
                OpString = opString;
                Type = type;
            }

            public string OpString { get; private set; }
            public VarType Type { get; private set; }

            public ValueContainer Calculate(ValueContainer value)
            {
                if (value == null || value.Type == VarType.Null)
                    return new ValueContainer();

                return _function(value);
            }
        }
    }
}