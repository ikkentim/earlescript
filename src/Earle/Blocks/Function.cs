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
using System.Linq;
using Earle.Variables;

namespace Earle.Blocks
{
    public class Function : Block
    {
        private readonly string[] _parameters;

        public Function(Block parent, string name, string[] parameters)
            : base(parent)
        {
            Name = name;
            _parameters = parameters;
        }

        public string Name { get; private set; }

        public virtual ValueContainer Invoke(params ValueContainer[] values)
        {
            if (_parameters.Length != values.Length)
                throw new Exception("Invalid argument count");

            Variables.Clear();
            foreach (var pair in _parameters.Zip(values, (p, v) => new KeyValuePair<string, ValueContainer>(p, v)))
                Variables.Add(pair.Key, pair.Value);

            return Children.Select(block => block.Run()).FirstOrDefault(value => value != null);
        }

        #region Overrides of Block

        public override ValueContainer Run()
        {
            throw new NotImplementedException("Function cannot be invoked using the Run method.");
        }

        #endregion

        #region Overrides of Object

        public override string ToString()
        {
            return string.Format("{0}::{1}", Path, Name);
        }

        #endregion
    }
}