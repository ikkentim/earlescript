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
using Earle.Blocks;
using Earle.Variables;

namespace Earle
{
    public class EarleFile : Block
    {
        private readonly Engine _engine;
        private readonly List<Function> _functions = new List<Function>();
        private readonly string _path;

        public EarleFile(Engine engine, string path) : base(null)
        {
            if (engine == null) throw new ArgumentNullException("engine");
            if (path == null) throw new ArgumentNullException("path");

            _engine = engine;
            _path = path;
        }

        public IList<Function> Functions
        {
            get { return _functions; }
        }

        public Function ResolveFunction(string path, string name)
        {
            return _functions.FirstOrDefault(m => m.Path == path && m.Name == name);
        }

        #region Overrides of Block

        public override bool IsReturnStatement
        {
            get { return false; }
        }

        public override string Path
        {
            get { return _path; }
        }

        public override Function ResolveFunction(FunctionCall functionCall)
        {
            return ResolveFunction(functionCall.CallPath ?? functionCall.Path, functionCall.Name) ??
                   (functionCall.CallPath == functionCall.Path ? null : _engine.ResolveFunction(functionCall));
        }

        public override ValueContainer ResolveVariable(string name)
        {
            return _engine.ResolveVariable(name);
        }

        public override ValueContainer Run()
        {
            var m = ResolveFunction(Path, "entry");

            if (m == null)
                throw new Exception();

            return m.Invoke();
        }

        #endregion
    }
}