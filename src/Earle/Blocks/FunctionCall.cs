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
    public class FunctionCall : Expression
    {
        private readonly Expression[] _arguments;

        public FunctionCall(Block parent, string callPath, string name, params Expression[] arguments)
            : base(parent)
        {
            if (name == null) throw new ArgumentNullException("name");
            if (arguments == null) throw new ArgumentNullException("arguments");
            CallPath = callPath;
            Name = name;
            _arguments = arguments;
        }

        public string CallPath { get; private set; }
        public string Name { get; private set; }

        #region Overrides of Object

        public override string ToString()
        {
            return string.Format("CALL {0}::{1}({2})", CallPath, Name, string.Join(", ", (object[]) _arguments));
        }

        #endregion

        #region Overrides of Block

        public override bool CanReturn
        {
            get { return false; }
        }

        public override ValueContainer Run()
        {
            var function = Parent.ResolveFunction(this);

            if (function == null)
                throw new RuntimeException(string.Format("Function `{0}::{1}` not found", CallPath, Name));

            return function.Invoke(_arguments.Select(a => a.Run()).ToArray());
        }

        #endregion
    }
}