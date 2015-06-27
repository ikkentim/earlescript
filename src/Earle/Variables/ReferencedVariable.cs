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
using Earle.Blocks;

namespace Earle.Variables
{
    public class ReferencedVariable : ValueContainer
    {
        private readonly Block _block;

        public ReferencedVariable(Block block, string name)
        {
            Name = name;
            if (block == null) throw new ArgumentNullException("block");
            _block = block;
        }

        public string Name { get; private set; }

        #region Overrides of ValueContainer

        public override VarType Type
        {
            get { return (_block.ResolveVariable(Name) ?? new ValueContainer(VarType.Null, null)).Type; }
        }

        public override object Value
        {
            get { return (_block.ResolveVariable(Name) ?? new ValueContainer(VarType.Null, null)).Value; }
        }

        public override void SetValue(ValueContainer value)
        {
            throw new Exception("A ReferencedVariable cannot be set");
        }

        #endregion
    }
}