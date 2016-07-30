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
using System.Reflection;

namespace EarleCode.Runtime
{
    public partial class EarleRuntime : EarleRuntimeScope
    {
        private readonly Dictionary<string, EarleFile> _files = new Dictionary<string, EarleFile>();
        private readonly EarleFunctionTable _natives = new EarleFunctionTable();
        private readonly Queue<EarleThread> _threadPool = new Queue<EarleThread>();

        public EarleRuntime() : base(null)
        {
            Compiler = new EarleCompiler(this);

            RegisterDefaultNatives();
        }

        internal EarleCompiler Compiler { get; }

        public EarleFile this[string fileName]
        {
            get { return GetFile(fileName); }
        }

        #region Natives

        public void RegisterNative(EarleFunction native)
        {
            if (native == null) throw new ArgumentNullException(nameof(native));

            _natives.Add(native);
        }

        public void RegisterNativesInType<T>()
        {
            RegisterNativesInType(typeof(T));
        }

        public void RegisterNativesInType(Type type)
        {
            if(type == null)
                throw new ArgumentNullException(nameof(type));
            foreach(var method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
            {
                var attribute = method.GetCustomAttributes(typeof(EarleNativeFunctionAttribute), true)?.FirstOrDefault() as EarleNativeFunctionAttribute;

                if(attribute == null)
                    continue;

                var name = attribute.Name?.ToLower() ?? method.Name.ToLower();

                RegisterNative(EarleNativeFunction.Create(name, null, method));
            }
        }

        #endregion

        #region Running

        private EarleValue? RunThread(EarleThread thread)
        {
            var result = thread.Frame.Run();

            if(result == null)
            {
                if(thread.IsAlive)
                    _threadPool.Enqueue(thread);
            }
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
                var thread = _threadPool.Dequeue();

                if(!thread.IsAlive)
                {
                    ticks--;
                    continue;
                }
                RunThread(thread);
                count++;
            }

            return !_threadPool.Any();
        }
        #endregion

        public virtual void HandleWarning(string warning)
        {
            Console.WriteLine(warning);
        }

        #region Files

        public void AddFile(EarleFile file)
        {
            if (file == null) throw new ArgumentNullException(nameof(file));

            if(_files.ContainsKey(file.Name))
                throw new ArgumentException("File name is already present");
            
            _files[file.Name] = file;
        }

        public bool RemoveFile(string fileName)
        {
            return _files.Remove(fileName);
        }

        public EarleFile GetFile(string fileName)
        {
            EarleFile file;
            _files.TryGetValue(fileName, out file);
            return file;
        }

        public EarleFile CompileFile(string fileName, string script)
        {
            var file = Compiler.CompileFile(fileName, script);

            AddFile(file);
            return file;
        }

        #endregion

        #region Invoking

        public EarleValue? Invoke(EarleFunction function, EarleCompletionHandler completionHandler, EarleValue target, IEnumerable<EarleValue> args)
        {
            if (function == null) throw new ArgumentNullException(nameof(function));

            var thread = new EarleThread(completionHandler);
            var rootFrame = new EarleStackFrame(this, thread, target);
            var frame = function.CreateFrameExecutor(rootFrame, target, args?.ToArray() ?? new EarleValue[0]);
            thread.AttachFrame(frame);

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

            var natives = _natives.Get(reference.Name);
            if(natives != null)
                return new EarleValue(natives);

            // TODO: Check global variables

            return EarleValue.Undefined;
        }

        protected override bool CanAssignReferenceInScope(EarleVariableReference reference)
        {
            return false;
        }

        #endregion
    }
}