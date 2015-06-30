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
    public class Assignment : Block
    {
        private readonly string _name;
        private readonly Expression _value;
        private readonly Expression[] _indexers;

        public Assignment(Block parent, string name, Expression value, params Expression[] indexers)
            : base(parent)
        {
            if (name == null) throw new ArgumentNullException("name");
            if (value == null) throw new ArgumentNullException("value");

            _name = name;
            _value = value;
            _indexers = indexers;

            value.Parent = this;
        }

        #region Overrides of Block

        public override ValueContainer Run()
        {
            var variable = ResolveVariable(_name) ?? AddVariable(_name);

            if(_indexers != null)
                foreach (var indexer in _indexers.Where(indexer => indexer != null))
                {
                    variable = variable[indexer.Run()];
                    if (variable == null)
                        return new ValueContainer();
                }

            var result = _value.Run();
            variable.Value = result == null ? null : result.Value;
            return result;
        }

        #endregion
    }
}