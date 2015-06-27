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
using System.Configuration;
using System.Globalization;
using Earle.Parsers;
using Earle.Variables;

namespace Earle.Blocks
{
    public abstract class Expression : Block
    {
        protected Expression(Block parent) : base(parent)
        {
        }

        #region Overrides of Block

        public override bool IsReturnStatement
        {
            get { return false; }
        }

        #endregion
    }

    public class VariableExpression : Expression
    {
        private readonly string _name;

        public VariableExpression(Block parent, string name) : base(parent)
        {
            if (name == null) throw new ArgumentNullException("name");
            _name = name;
        }

        #region Overrides of Block

        public override ValueContainer Run()
        {
            return ResolveVariable(_name);
        }

        #endregion
    }

    public class OperatorExpression : Expression
    {
        public Expression Left { get; set; }
        public string OP { get; set; }
        public Expression Right { get; set; }

        public OperatorExpression(Block parent) : base(parent)
        {
        }

        public OperatorExpression(Block parent, Expression left, string op, Expression right) : base(parent)
        {
            Left = left;
            OP = op;
            Right = right;
        }

        #region Overrides of Block

        public override ValueContainer Run()
        {
            var leftValue = Left == null ? null : Left.Run();
            if (leftValue == null) 
                return null;
            var rightValue = Right == null ? null : Right.Run();

            switch (leftValue.Type)
            {
                case VarType.Null:
                    return null;
                case VarType.Number:
                    var num = (float) leftValue.Value;
                    switch (OP)
                    {
                        case "+":
                            switch (rightValue.Type)
                            {
                                case VarType.Null:
                                    return null;
                                case VarType.Number:
                                    return new ValueContainer(num + (float) rightValue.Value);
                                case VarType.Object:
                                    throw new NotImplementedException();
                                case VarType.String:
                                    return new ValueContainer(num.ToString(CultureInfo.InvariantCulture) + rightValue);
                                case VarType.Target:
                                    throw new NotImplementedException();
                                default:
                                    throw new RuntimeException("Unsupported variable type");
                            }
                        default:
                            throw new RuntimeException(string.Format("Unsupported number operator `{0}` called", OP));
                    }
                case VarType.Object:
                    throw new NotImplementedException();
                case VarType.String:
                    var str = leftValue.ToString();
                    switch (OP)
                    {
                        case "+":
                            return new ValueContainer(str + rightValue);
                        default:
                            throw new RuntimeException(string.Format("Unsupported string operator `{0}` called", OP));
                    }
                case VarType.Target:
                    throw new NotImplementedException();
                default:
                    throw new RuntimeException("Unsupported variable type");
            }
        }

        #endregion
    }

    public class ValueExpression : Expression
    {
        private readonly ValueContainer _value;

        public ValueExpression(Block parent, ValueContainer value) : base(parent)
        {
            _value = value;
        }

        #region Overrides of Block

        public override ValueContainer Run()
        {
            return _value;
        }

        #endregion
    }
}