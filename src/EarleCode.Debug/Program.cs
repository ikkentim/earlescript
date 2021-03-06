﻿// EarleCode
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
using System.Diagnostics;
using System.IO;
using System.Threading;
using EarleCode.Runtime;
using EarleCode.Runtime.Events;
using EarleCode.Runtime.Values;

namespace EarleCode.Debug
{
	public class Program
	{
		private static string GetRelativePath(string filespec, string folder)
		{
			var pathUri = new Uri(filespec);

			if (!folder.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal))
				folder += Path.DirectorySeparatorChar;

			var folderUri = new Uri(folder);
			return Uri.UnescapeDataString(folderUri.MakeRelativeUri(pathUri).ToString().Replace('/', Path.DirectorySeparatorChar));
		}

		private static void Main(string[] args)
		{
			//Console.WriteLine(4 << 1 + 1);//16
			//Console.WriteLine(4 | 1 << 1 + 1);//4
			//Console.WriteLine(4 * 3 == 12);//true
			//Console.WriteLine(3 & 1 | 2 << 2);//9
			//Console.WriteLine(1 < 3 == 3 < 1);//false

			var runtime = new EarleRuntime();
			runtime.GlobalVariables["level"] = new EarleSimpleEventableStructure().ToEarleValue();

			// Load code
			var swc = Stopwatch.StartNew();
			var codeDir = Path.Combine(Directory.GetCurrentDirectory(), "code");
			foreach (var file in Directory.GetFiles(codeDir, "*.earle", SearchOption.AllDirectories))
			{
				var rel = GetRelativePath(file, codeDir).Replace('/', '\\');
				rel = '\\' + rel.Substring(0, rel.Length - 6);
				runtime.CompileFile(rel, File.ReadAllText(file));
			}
			swc.Stop();
			Console.WriteLine($"Compiling took: {swc.Elapsed}");

			// Add localization
			foreach (var file in Directory.GetFiles(codeDir, "*.estr", SearchOption.AllDirectories))
			{
				var rel = GetRelativePath(file, codeDir).Replace('/', '\\');
				rel = rel.Substring(0, rel.Length - 5);
				runtime.Localizer.LoadFromFile(rel, File.ReadAllText(file), Path.GetFileNameWithoutExtension(file).ToUpper() + "_");
			}
			runtime.Localizer.Key = "LANG_ENGLISH";

			foreach (var l in runtime.GetDebugInformation())
				Console.WriteLine(l);

			runtime.Natives.Register(EarleNativeFunction.Create("printstacktrace", (EarleStackFrame frame) =>
			{
				var trace = frame.GetStackTrace();
				Console.WriteLine(trace);
			}));

			var test = runtime["\\main"]["test"];

			if (test != null)
			{
				var count = runtime["\\main"]["gettestcount"]?
					.Invoke(null, EarleValue.Undefined)?
					.CastTo<int>() ?? 10000;

				var sw = Stopwatch.StartNew();
				for (var i = 0; i < count; i++)
				{
					test.Invoke(null, EarleValue.Undefined);
				}
				sw.Stop();

				Console.WriteLine($"Running {count} times took {sw.Elapsed}");
			}
			else
			{
				// Invoke main::init
				runtime["\\main"]["init"].Invoke(result =>
				{
					Console.WriteLine();
					Console.WriteLine("Code execution completed!");
					Console.WriteLine("Result: " + result);

					if (result.Is<EarleStructure>())
					{
						var struc = result.As<EarleStructure>();
						foreach (var kv in struc)
							Console.WriteLine($"> {kv.Key} = {kv.Value}");
					}
					else if (result.Is<EarleArray>())
					{
						var arr = result.As<EarleArray>();
						foreach (var v in arr)
							Console.WriteLine(v);
					}
				}, EarleValue.Undefined);

				do
				{
					Thread.Sleep(1000/30);
				} while (!runtime.Tick());
			}

			Console.WriteLine("Done!");
		}
	}
}