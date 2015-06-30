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

using System.Collections.Generic;
using System.Linq;
using Earle.Blocks.Expressions;
using Earle.Variables;

namespace Earle.Blocks
{
    public class ForStatement : Block
    {
        private readonly Expression _condition;
        private readonly Assignment _incrementation;
        private readonly Assignment _initialisation;
        private readonly Dictionary<string, ValueContainer> _variables = new Dictionary<string, ValueContainer>();

        public ForStatement(Block parent, Assignment initialisation, Expression condition, Assignment incrementation)
            : base(parent, true)
        {
            _initialisation = initialisation;
            _condition = condition;
            _incrementation = incrementation;

            if (initialisation != null)
                initialisation.Parent = this;

            if (condition != null)
                condition.Parent = this;

            if (incrementation != null)
                incrementation.Parent = this;
        }

        #region Overrides of Block

        public override ValueContainer AddVariable(string name)
        {
            return _variables[name] = new ValueContainer();
        }

        public override ValueContainer ResolveVariable(string name)
        {
            return base.ResolveVariable(name) ?? (_variables.ContainsKey(name) ? _variables[name] : null);
        }

        public override ValueContainer Run()
        {
            if (_initialisation != null)
                _initialisation.Run();

            while (true)
            {
                if (_condition != null)
                {
                    var result = _condition.Run();

                    if (result == null || result.Type == VarType.Null ||
                        (result.Type == VarType.Integer && (int) result == 0) ||
                        (result.Type == VarType.Float && (float) result == 0) ||
                        (result.Type == VarType.Object && result.Value != null) ||
                        (result.Type == VarType.String && string.IsNullOrEmpty(result.Value as string)) ||
                        (result.Type == VarType.Target && result.Value == null))
                        return null;
                }

                foreach (
                    var value in
                        Children.Select(block => new {block, value = block.Run()})
                            .Where(a => a.value != null && a.block.CanReturn)
                            .Select(a => a.value))
                {
                    return value;
                }

                if (_incrementation != null)
                    _incrementation.Run();
            }

            #endregion
        }
    }
}