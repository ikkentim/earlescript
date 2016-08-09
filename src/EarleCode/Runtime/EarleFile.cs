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
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using EarleCode.Runtime.Values;

namespace EarleCode.Runtime
{
    public class EarleFile : EarleRuntimeScope, IEnumerable<EarleFunction>
    {
        private EarleFunctionTable _functions = new EarleFunctionTable();
        private readonly List<string> _includedFiles = new List<string>();
        private readonly List<string> _referencedFiles = new List<string>();
        private readonly List<EarleValue> _valueStore = new List<EarleValue>();

        public EarleFile(EarleRuntime runtime, string name) : base(runtime)
        {
            if (runtime == null) throw new ArgumentNullException(nameof(runtime));
            if (name == null) throw new ArgumentNullException(nameof(name));

            Runtime = runtime;
            Name = name;
        }

        public string Name { get; }

        public EarleRuntime Runtime { get; }

        public EarleFunctionCollection this[string functionName] => GetFunctions(functionName);

        public IEnumerable<string> ReferencedFiles => _referencedFiles.AsReadOnly(); 

        public IEnumerable<string> IncludedFiles => _includedFiles.AsReadOnly();

        public void IncludeFile(string file)
        {
            if(file == null) throw new ArgumentNullException(nameof(file));
            if(!_includedFiles.Contains(file))
                _includedFiles.Add(file);
        }

        public void AddFunction(EarleFunction function)
        {
            if (function == null) throw new ArgumentNullException(nameof(function));

            _functions.Add(function);
        }

        public void AddReferencedFile(string fileName)
        {
            if(fileName == null) throw new ArgumentNullException(nameof(fileName));
            
            if(!_referencedFiles.Contains(fileName))
                _referencedFiles.Add(fileName);
        }

        public void AddReferencedFiles(string[] fileNames)
        {
            if(fileNames == null) throw new ArgumentNullException(nameof(fileNames));

            foreach(var f in fileNames)
                AddReferencedFile(f);
        }

        public EarleFunctionCollection GetFunctions(string functionName)
        {
            return _functions.Get(functionName);
        }

        public int GetIndexForValueInStore(EarleValue value)
        {
            var index = _valueStore.IndexOf(value);

            if(index < 0)
            {
                index = _valueStore.Count;
                _valueStore.Add(value);
            }

            return index;
        }

        public EarleValue GetValueInStore(int index)
        {
            if(index < 0 || index >= _valueStore.Count)
                return EarleValue.Undefined;

            return _valueStore[index];
        }

        public EarleValue? Invoke(string functionName, EarleCompletionHandler completionHandler, EarleValue target, params EarleValue[] arguments)
        {
            return GetFunctions(functionName).Invoke(completionHandler, target, arguments);
        }

        #region Overrides of RuntimeScope

        protected override bool CanAssignVariableInScope(string name)
        {
            return false;
        }

        public override EarleFunctionCollection GetFunctionReference(string fileName, string functionName)
        {
            // FIXME: This might mess up function tables of other files when things are added. Should a readonly collection be returned?
            var result = Runtime.GetFunctionReference(fileName, functionName);

            if(fileName == null || fileName == Name)
            {
                var functions = GetFunctions(functionName);

                if(functions != null)
                {
                    if(result != null)
                        result.AddRange(functions);
                    else
                        result = functions;
                }

                foreach(var include in IncludedFiles)
                {
                    var file = Runtime.GetFile(include);

                    if(file == null)
                        continue;

                    var funcs = file.GetFunctions(functionName);

                    if(funcs == null)
                        continue;

                    if(result != null)
                        result.AddRange(funcs);
                    else
                        result = funcs;
                }
            }

            return result;
        }

        #endregion

        #region Implementation of IEnumerable<EarleFunction>

        public IEnumerator<EarleFunction> GetEnumerator()
        {
            return _functions.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}