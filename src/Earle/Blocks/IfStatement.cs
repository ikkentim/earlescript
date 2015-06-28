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
using Earle.Blocks.Expressions;
using Earle.Variables;

namespace Earle.Blocks
{
    public class IfStatement : Block
    {
        private readonly Expression _expression;

        public IfStatement(Block parent, Expression expression) : base(parent, true)
        {
            if (expression == null) throw new ArgumentNullException("expression");

            _expression = expression;

            expression.Parent = this;
        }

        #region Overrides of Block

        public override ValueContainer Run()
        {
            var result = _expression.Run();

            if (result == null || result.Type == VarType.Null ||
                (result.Type == VarType.Integer && (int) result == 0) ||
                (result.Type == VarType.Float && (float) result == 0) ||
                (result.Type == VarType.Object && result.Value != null) ||
                (result.Type == VarType.String && string.IsNullOrEmpty(result.Value as string)) ||
                (result.Type == VarType.Target && result.Value == null))
                return null;

            return Children.Select(block => new {block, value = block.Run()})
                .Where(a => a.value != null && a.block.CanReturn)
                .Select(a => a.value).FirstOrDefault();
        }

        #endregion
    }
}