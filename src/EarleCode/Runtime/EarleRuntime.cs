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
using System.Linq;
using EarleCode.Compiler;
using EarleCode.Runtime.Values;

namespace EarleCode.Runtime
{
    public partial class EarleRuntime : EarleRuntimeScope
    {
        private readonly Dictionary<string, EarleFile> _files = new Dictionary<string, EarleFile>();

        private readonly Dictionary<string, EarleFunctionCollection> _natives =
            new Dictionary<string, EarleFunctionCollection>();

        private readonly Queue<EarleThread> _threadPool = new Queue<EarleThread>();

        public EarleRuntime() : base(null)
        {
            Compiler = new EarleCompiler(this);

            RegisterDefaultNatives();
        }

        public EarleCompiler Compiler { get; }

        #region Natives

        public void RegisterNative(EarleFunction native)
        {
            if (native == null) throw new ArgumentNullException(nameof(native));

            EarleFunctionCollection collection;
            if (!_natives.TryGetValue(native.Name, out collection))
                _natives[native.Name] = collection = new EarleFunctionCollection();

            collection.Add(native);
        }

        #endregion

        #region Running

        private EarleValue? RunThread(EarleThread thread)
        {
            var result = thread.Frame.Run();

            if(result == null)
                _threadPool.Enqueue(thread);
            else if(thread.CompletionHandler != null)
                thread.CompletionHandler(result.Value);
            
            return result;
        }

        public bool Tick(int ticks = int.MaxValue)
        {
            if(_threadPool.Count < ticks)
                ticks = _threadPool.Count;
            
            int count = 0;
            while(_threadPool.Any() && count < ticks)
            {
                RunThread(_threadPool.Dequeue());
            }

            return !_threadPool.Any();
        }
        #endregion

        public virtual void HandleWarning(string warning)
        {
            Console.WriteLine(warning);
        }

        #region Value types

        //public void RegisterValueType(IEarleValueType valueType)
        //{
        //    if (valueType == null) throw new ArgumentNullException(nameof(valueType));
        //    _valueTypes[valueType.Type] = valueType;
        //}

        //public IEarleValueType GetValueTypeForType(Type type)
        //{
        //    if(type == null)
        //        return null;
            
        //    IEarleValueType valueType;
        //    return _valueTypes.TryGetValue(type, out valueType) ? valueType : null;
        //}

        #endregion

        #region Files

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

        #endregion

        #region Invoking

        public EarleValue? Invoke(EarleFunction function, IEnumerable<EarleValue> arguments, EarleValue target, EarleCompletionHandler completionHandler = null)
        {
            if (function == null) throw new ArgumentNullException(nameof(function));

            var rootFrame = new EarleStackFrame(this, EarleValue.Undefined);
            var frame = function.CreateFrameExecutor(rootFrame, target, arguments?.ToArray() ?? new EarleValue[0]);
            var thread = new EarleThread(frame, completionHandler);

            return RunThread(thread);
        }

        public void StartThread(EarleThread thread)
        {
            _threadPool.Enqueue(thread);
        }

        #endregion

        #region Overrides of RuntimeScope

        public override EarleValue GetValue(EarleVariableReference reference)
        {
            if (!string.IsNullOrEmpty(reference.File))
                return GetFile(reference.File)?.GetFunctions(reference.Name)?.ToEarleValue() ?? EarleValue.Undefined;

            EarleFunctionCollection natives;
            if (_natives.TryGetValue(reference.Name, out natives))
                return new EarleValue(natives);

            // TODO: Check global variables

            return EarleValue.Undefined;
        }

        protected override bool CanAssignReferenceAsLocal(EarleVariableReference reference)
        {
            return false;
        }

        #endregion
    }
}