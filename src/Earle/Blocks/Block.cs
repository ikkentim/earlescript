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
using System.Collections.Generic;
using Earle.Blocks.Expressions;
using Earle.Variables;

namespace Earle.Blocks
{
    public abstract class Block
    {
        private readonly List<Block> _children;
        private readonly Dictionary<string, ValueContainer> _variables = new Dictionary<string, ValueContainer>();

        protected Block(Block parent, bool canReturn = false)
        {
            _children = new List<Block>();
            Parent = parent;
            CanReturn = canReturn;
        }

        public virtual Block Parent { get; set; }
        public virtual bool CanReturn { get; private set; }

        public virtual string Path
        {
            get
            {
                if (Parent == null)
                    throw new Exception();
                return Parent.Path;
            }
        }

        public virtual IEnumerable<Block> Children
        {
            get { return _children; }
        }

        public virtual Dictionary<string, ValueContainer> Variables
        {
            get { return _variables; }
        }

        public virtual void AddBlock(Block block)
        {
            if (block == null) throw new ArgumentNullException("block");
            _children.Add(block);
        }

        public abstract ValueContainer Run();

        public virtual Function ResolveFunction(FunctionCall functionCall)
        {
            return Parent != null ? Parent.ResolveFunction(functionCall) : null;
        }

        public virtual ValueContainer ResolveVariable(string name)
        {
            if (name == null) throw new ArgumentNullException("name");
            // Breath-first search for variable.
            ValueContainer value;
            _variables.TryGetValue(name, out value);

            return Parent != null ? Parent.ResolveVariable(name) ?? value : value;
        }

        public virtual ValueContainer AddVariable(string name)
        {
            if (name == null) throw new ArgumentNullException("name");
            return Parent.AddVariable(name);
        }
    }
}