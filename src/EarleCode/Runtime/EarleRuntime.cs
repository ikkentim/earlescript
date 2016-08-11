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
using System.Linq;
using EarleCode.Compiler;
using EarleCode.Localization;
using EarleCode.Runtime.Attributes;
using EarleCode.Runtime.Events;
using EarleCode.Runtime.Operators;
using EarleCode.Runtime.Values;

namespace EarleCode.Runtime
{
	public partial class EarleRuntime : IEarleRuntimeScope, IEnumerable<EarleFile>
	{
		private readonly Dictionary<string, EarleFile> _files = new Dictionary<string, EarleFile>();
		private readonly Queue<EarleThread> _threadPool = new Queue<EarleThread>();

		public EarleRuntime()
		{
			Compiler = new EarleCompiler(this);

			RegisterDefaultNatives();
			RegisterDefaultOperators();
		}

		internal EarleCompiler Compiler { get; }

		public EarleDictionary GlobalVariables { get; } = new EarleDictionary();

		public EarleOperatorCollection Operators { get; } = new EarleOperatorCollection();

		public EarleLocalizer Localizer { get; } = new EarleLocalizer();

		public EarleNativeCollection Natives { get; } = new EarleNativeCollection();

		public EarleFile this[string fileName] => GetFile(fileName);

		private void RegisterDefaultNatives()
		{
		    var scanner = new EarleAttributeScanner(this);
            scanner.Scan<EarleDefaultNatives>();
            scanner.Scan<EarleEventManagerNatives>();
            scanner.Scan<EarleLocalizerNatives>();
		}

		#region Debugging

		public IEnumerable<string> GetDebugInformation()
		{
			return this.SelectMany(f => f).SelectMany(Compiler.GetDebugInformation);
		}

		#endregion

		#region Running

		public void EnqueueThread(EarleThread thread)
		{
			_threadPool.Enqueue(thread);
		}

		public bool Tick(int ticks = int.MaxValue)
		{
			if (_threadPool.Count < ticks)
				ticks = _threadPool.Count;

			int count = 0;
			while (_threadPool.Any() && count < ticks)
			{
				var thread = _threadPool.Dequeue();

				if (!thread.IsAlive)
				{
					ticks--;
					continue;
				}
				thread.Run();
				count++;
			}

			return !_threadPool.Any();
		}

		public virtual void HandleWarning(string warning)
		{
			// TODO: Implement proper warning/error handling
			Console.WriteLine(warning);
		}

		#endregion

		#region Files

		public void AddFile(EarleFile file)
		{
			if (file == null) throw new ArgumentNullException(nameof(file));

			if (_files.ContainsKey(file.Name))
				throw new ArgumentException("File name is already present");

			file.Lock();
			_files[file.Name] = file;

			foreach (var f in this)
				f.ClearCache();
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
			//try
			//{
			var file = Compiler.CompileFile(fileName, script);

			AddFile(file);
			return file;
			//}
			//catch(Exception e)
			//{
			//    throw e;
			//}
		}

		#endregion

		#region Implementation of IRuntimeScope

		public virtual EarleValue GetValue(string name)
		{
			// Look global variables up.
			if (GlobalVariables.ContainsKey(name))
				return GlobalVariables[name];

			return EarleValue.Undefined;
		}

		public virtual bool SetValue(string name, EarleValue value)
		{
			return false;
		}

		public virtual EarleFunctionCollection GetFunctionReference(string fileName, string functionName)
		{
			if (fileName == null)
			{
				return Natives.Get(functionName);
			}

			var file = GetFile(fileName);

			return file == null
				? null
				: file.GetFunctions(functionName);
		}

		#endregion

		#region Implementation of IEnumerable<EarleFile>

		public IEnumerator<EarleFile> GetEnumerator()
		{
			return _files.Values.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion
	}
}