// EarleCode
// Copyright 2018 Tim Potze
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
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using EarleCode.Compiling;

namespace EarleCode.Debug
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			CleanRun();
		}


		private static void CleanRun()
		{
			// Load the interpreter with a function which indicates how script files are loaded
			var interpreter = new EarleInterpreter(path => path.IsEmpty
				? null
				: File.ReadAllText($"scripts/{path.RelativePath}.earle"));

			// Register a simple "print" function
			interpreter.Natives.Register("print", 1, (a) =>
			{
				Console.WriteLine(a[0].Convert(EarleValueType.String).StringValue);
				return EarleValue.Null;
			});

			// Register a "waittick" function which halts execution for one game tick
			interpreter.Natives.Register("waittick", 0, (Func<EarleValue[], IEnumerator>) WaitOneFrame);

			// Invoke the "main" method in the "waitsample" script file
			interpreter.Invoke(interpreter["\\waitsample"]["main"], out _);

			// ... mock game update loop
			for (;;)
			{
				interpreter.Tick();

				Thread.Sleep(100);
			}
		}

		private static IEnumerator WaitOneFrame(EarleValue[] args)
		{
			yield return null;
		}

		private static void PerformanceTest()
		{
			var sw = new Stopwatch();

			sw.Start();
			var interpreter = new EarleInterpreter(path => path.IsEmpty
				? null
				: File.ReadAllText($"scripts/{path.RelativePath}.earle"));

			interpreter.Natives.Register("print", 1, (a) =>
			{
				Console.WriteLine(a[0].Convert(EarleValueType.String).StringValue);
				return EarleValue.Null;
			});

			sw.Stop();
			var buildInterpreter = sw.Elapsed;

			sw.Restart();
			var mainFn = interpreter["\\main"]["main"];
			sw.Stop();
			var compiling = sw.Elapsed;

			sw.Restart();
			var ok = interpreter.Invoke(mainFn, out var res);
			sw.Stop();
			var running = sw.Elapsed;

			Console.WriteLine($"OK: {ok}, result: {res}");
			Console.WriteLine("Building interpreter: " + buildInterpreter);
			Console.WriteLine("Compiling: " + compiling);
			Console.WriteLine("Running: " + running);
		}
	}
}