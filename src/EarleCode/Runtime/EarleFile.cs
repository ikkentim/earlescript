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
        private Dictionary<Tuple<string, string>, EarleFunctionCollection> _functionsCache = new Dictionary<Tuple<string, string>, EarleFunctionCollection>();

        public EarleFile(EarleRuntime runtime, string name) : base(runtime)
        {
            if (runtime == null) throw new ArgumentNullException(nameof(runtime));
            if (name == null) throw new ArgumentNullException(nameof(name));

            Runtime = runtime;
            Name = name;
        }

        public string Name { get; }

        public bool Locked { get; private set; }

        public EarleRuntime Runtime { get; }

        public EarleFunctionCollection this[string functionName] => GetFunctions(functionName);

        public IEnumerable<string> ReferencedFiles => _referencedFiles.AsReadOnly(); 

        public IEnumerable<string> IncludedFiles => _includedFiles.AsReadOnly();

        public void IncludeFile(string file)
        {
            if(file == null) throw new ArgumentNullException(nameof(file));

            if(Locked)
                throw new Exception("Cannot include a file while this instance is locked.");
            
            if(!_includedFiles.Contains(file))
                _includedFiles.Add(file);
        }

        public void AddFunction(EarleFunction function)
        {
            if (function == null) throw new ArgumentNullException(nameof(function));

            if(Locked)
                throw new Exception("Cannot add a function while this instance is locked.");
            
            _functions.Add(function);
        }

        public void AddReferencedFile(string fileName)
        {
            if(fileName == null) throw new ArgumentNullException(nameof(fileName));

            if(Locked)
                throw new Exception("Cannot add a referenced file while this instance is locked.");
            
            if(!_referencedFiles.Contains(fileName))
                _referencedFiles.Add(fileName);
        }

        public void AddReferencedFiles(string[] fileNames)
        {
            if(fileNames == null) throw new ArgumentNullException(nameof(fileNames));

            if(Locked)
                throw new Exception("Cannot add a referenced file while this instance is locked.");

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

        public void Lock()
        {
            Locked = true;
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

        public void ClearCache()
        {
            _functionsCache.Clear();
        }

        #region Overrides of RuntimeScope

        protected override bool CanAssignVariableInScope(string name)
        {
            return false;
        }

        public override EarleFunctionCollection GetFunctionReference(string fileName, string functionName)
        {
            if(functionName == null) throw new ArgumentNullException(nameof(functionName));

            var tuple = new Tuple<string, string>(fileName, functionName);

            EarleFunctionCollection result;

            if(Locked && _functionsCache.TryGetValue(tuple, out result))
                return result;

            result = new EarleFunctionCollection();

            var runtimeFunctions = Runtime.GetFunctionReference(fileName, functionName);
            if(runtimeFunctions != null)
                result.AddRange(runtimeFunctions);
            
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

            if(Locked)
                _functionsCache[tuple] = result;
            
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