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
using System.IO;
using System.Linq;
using Earle.Blocks;
using Earle.Variables;

namespace Earle
{
    public class Engine
    {
        private readonly Compiler _compiler;
        private readonly Dictionary<string, EarleFile> _files = new Dictionary<string, EarleFile>();
        private readonly Dictionary<string, ValueContainer> _globalVariables = new Dictionary<string, ValueContainer>();

        private readonly List<Function> _nativeFunctions = new List<Function>
        {
            new NativeFunction("print",
                p =>
                {
                    foreach (var v in p)
                        Console.WriteLine(v.Value);
                    return null;
                })
        };

        private PathResolverDelegate _pathResolver;

        public Engine(PathResolverDelegate pathResolver)
        {
            if (pathResolver == null) throw new ArgumentNullException("pathResolver");
            PathResolver = pathResolver;

            _compiler = new Compiler(this);
        }

        public PathResolverDelegate PathResolver
        {
            get { return _pathResolver; }
            set
            {
                if (value == null) throw new ArgumentNullException("value");
                _pathResolver = value;
            }
        }

        public EarleFile this[string path]
        {
            get
            {
                if (!IsFileLoaded(path))
                    LoadFile(path);

                return _files[path];
            }
        }

        public Function ResolveFunction(FunctionCall functionCall)
        {
            return functionCall.CallPath == null
                ? _nativeFunctions.FirstOrDefault(f => f.Name == functionCall.Name)
                : this[functionCall.CallPath].ResolveFunction(functionCall);
        }

        public ValueContainer ResolveVariable(string name)
        {
            return _globalVariables.ContainsKey(name) ? _globalVariables[name] : null;
        }

        public bool IsFileLoaded(string path)
        {
            if (path == null) throw new ArgumentNullException("path");
            return _files.ContainsKey(path);
        }

        public void LoadFile(string path)
        {
            if (path == null)
                throw new ArgumentNullException("path");

            if (_files.ContainsKey(path))
                throw new ArgumentException("Path has already been loaded", "path");

            using (var stream = PathResolver(path))
            {
                if (stream == null)
                {
                    throw new Exception();
                }
                using (var streamReader = new StreamReader(stream))
                {
                    var input = streamReader.ReadToEnd();
                    _files[path] = _compiler.Compile(path, input);
                }
            }
        }
    }
}