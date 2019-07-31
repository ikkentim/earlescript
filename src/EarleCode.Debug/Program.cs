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
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using EarleCode.Compiling;
using EarleCode.Compiling.Earle;
using EarleCode.Compiling.Lexing;
using EarleCode.Compiling.Parsing;
using EarleCode.Compiling.Parsing.Grammars;

namespace EarleCode.Debug
{
	internal class Program
	{
		private static void Main(string[] args)
		{
//			Serialization();
//			CleanRun();
			PerformanceTest();
		}

		private static void Serialization()
		{
			var grammar = new EnumGrammar<ProductionRuleEnum>(EarleCompiler.MultiCharSymbols);
			var slr = new SLRParsingTable(grammar);

			var ser = new CacheSerializer();

			Console.WriteLine("======== Grammar ========");
			using (var target = new MemoryStream())
			{
				using (var zipStream = new GZipStream(target, CompressionLevel.Optimal, true))
					ser.Serialize(grammar, zipStream);

				target.Seek(0, SeekOrigin.Begin);
				var serializedGrammar = target.ToArray();

				Console.WriteLine(Convert.ToBase64String(serializedGrammar));

				using (var zipStream = new GZipStream(target, CompressionMode.Decompress, true))
				{
					var newGram = ser.DeserializeGrammar(zipStream);
					Console.WriteLine(newGram);
				}
			}

			Console.WriteLine();
			Console.WriteLine("======== SLR Table ========");
			using (var target = File.OpenWrite("slr.earle-cache"))
			{
				ser.Serialize(slr, target);
			}

			Console.ReadLine();
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
			interpreter.Invoke(interpreter["\\waitsample"]["main"], EarleValue.Null, out _);

			// ... mock game update loop
			while (interpreter.Tick())
			{
				Thread.Sleep(100);
			}
		}

		private static IEnumerator WaitOneFrame(EarleValue[] args)
		{
			yield return null;
		}
		
		private static IEnumerator Wait(EarleValue[] args)
		{
			var sw = new Stopwatch();
			sw.Start();

			var delay = args[0];
			var delayF = delay.Type == EarleValueType.NumberFloat ? delay.FloatValue : (float) delay.IntValue;

			while (sw.Elapsed.TotalSeconds < delayF)
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
			
			// Register a "waittick" function which halts execution for one game tick
			interpreter.Natives.Register("waittick", 0, (Func<EarleValue[], IEnumerator>) WaitOneFrame);
			
			// Register a "wait" function to implement the wait statement
			interpreter.Natives.Register("wait", 1, (Func<EarleValue[], IEnumerator>) Wait);

			sw.Stop();
			var buildInterpreter = sw.Elapsed;

			sw.Restart();
			//var mainFn = interpreter["\\main"]["main"];
			var mainFn = interpreter["\\allfeatures"]["main"];

			sw.Stop();
			var compiling = sw.Elapsed;

			sw.Restart();
			var ok = interpreter.Invoke(mainFn, EarleValue.Null, out var res);

			if (!ok)
			{
				while (interpreter.Tick())
				{
					Thread.Sleep(50);
				}
			}
			
			sw.Stop();
			var running = sw.Elapsed;

			Console.WriteLine($"OK: {ok}, result: {res}");
			Console.WriteLine("Building interpreter: " + buildInterpreter);
			Console.WriteLine("Compiling: " + compiling);
			Console.WriteLine("Running: " + running);
		}
	}
}