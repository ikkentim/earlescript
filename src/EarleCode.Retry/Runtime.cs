// EarleCode
// Copyright 2016 Tim Potze
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
using System.Diagnostics;
using System.Linq;

namespace EarleCode.Retry
{
    public class Runtime : RuntimeScope
    {
        private readonly Dictionary<string, EarleFile> _files = new Dictionary<string, EarleFile>();
        private readonly Dictionary<string, NativeFunction> _natives = new Dictionary<string, NativeFunction>(); 
        public Runtime() : base(null)
        {
            Compiler = new Compiler(this);

            _natives["print"] = new NativeFunction("print", values =>
            {
                Console.WriteLine(values.FirstOrDefault().Value);
                return new EarleValue();
            }, "value");
        }

        public Compiler Compiler { get; }

        public void AddFile(EarleFile file)
        {
            if (file == null) throw new ArgumentNullException(nameof(file));
            _files[file.Name] = file;
        }

        public EarleFile CompileFile(string fileName, string script)
        {
            var file = Compiler.CompileFile(fileName, script);

            AddFile(file);
            return file;
        }

        public EarleFile GetFile(string fileName)
        {
            EarleFile file;
            _files.TryGetValue(fileName, out file);
            return file;
        }

        public void Invoke(EarleFunction function)
        {
            Invoke(function, null);
        }

        public void Invoke(EarleFunction function, IEnumerable<EarleValue> arguments)
        {
            if (function == null) throw new ArgumentNullException(nameof(function));

            var stack = arguments == null 
                ? new Stack<EarleValue>() 
                : new Stack<EarleValue>(arguments.Reverse());

            RunLoop(function.CreateLoop(this, stack));
        }

        public void Invoke(byte[] pCode)
        {
            if (pCode == null) throw new ArgumentNullException(nameof(pCode));
            
            RunLoop(new RuntimeLoop(this, this, pCode));
        }

        private void RunLoop(RuntimeLoop loop)
        {
            // TODO: Store loop if execution did not complete.
            loop.Run();
        }

        #region Overrides of RuntimeScope

        public override EarleValue? GetValue(EarleVariableReference reference)
        {
            if (!string.IsNullOrEmpty(reference.File))
            {
                var function = GetFile(reference.File)?.GetFunction(reference.Name);

                if (function != null)
                {
                    return new EarleValue(function);
                }
            }
            else
            {
                NativeFunction native;
                if (_natives.TryGetValue(reference.Name, out native))
                {
                    return new EarleValue(native);
                }
            }
            return null;
        }
        
        protected override bool CanAssignReferenceAsLocal(EarleVariableReference reference)
        {
            return false;
        }

        #endregion
    }
}