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

using Earle.Blocks.Expressions;
using Earle.Variables;

namespace Earle.Blocks
{
    public class Assignment : Block
    {
        private readonly string _name;
        private readonly Expression _value;

        public Assignment(Block parent, string name, Expression value) : base(parent)
        {
            _name = name;
            _value = value;
            value.Parent = this;
        }

        #region Overrides of Block

        public override bool CanReturn
        {
            get { return false; }
        }

        public override ValueContainer Run()
        {
            var variable = ResolveVariable(_name);

            if (variable == null)
                AddVariable(_name, _value.Run());
            else
                variable.SetValue(_value.Run());

            return null;
        }

        #endregion
    }
}