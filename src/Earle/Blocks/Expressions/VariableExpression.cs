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
using Earle.Variables;

namespace Earle.Blocks.Expressions
{
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
}