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
using System.Threading;
using CommandLine;
using EarleCode.Compiling;
using EarleCode.Compiling.Earle;
using EarleCode.Compiling.Lexing;
using EarleCode.Compiling.Parsing;
using EarleCode.Compiling.Parsing.Grammars;
using EarleCode.Interpreter;

namespace EarleCode.Debug
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			Options opts = null;
			Parser.Default.ParseArguments<Options>(args).WithParsed(x => { opts = x; });

			if (opts?.PopulateCache ?? false)
			{
				Console.WriteLine("Cache population started.");
				
				Serialization();

				return;
			}

			if ((opts?.ParserTest ?? false))
			{
				var g = new EnumGrammar<GrammarCompat>(null);
			
				TestParser("LL1", new LL1Parser(g));
				TestParser("SLR", new SLRParser(g));
				//TestParser("LALR", new LALRParser(g)); // FIXME: LALR Parser hardcoded to earle at the moment

				return;
			}

			Run(opts?.InputFile ?? "\\main", opts?.Function ?? "main", opts?.IgnorePrint ?? false);
		}

		private static void Serialization()
		{
			var grammar = new EnumGrammar<ProductionRuleEnum>(EarleCompiler.MultiCharSymbols);
			//var slr = new SLRParsingTable(grammar);

//			var ser = new CacheSerializer();
//
//			Console.WriteLine("======== Grammar ========");
//			using (var target = new MemoryStream())
//			{
//				using (var zipStream = new GZipStream(target, CompressionLevel.Optimal, true))
//					ser.Serialize(grammar, zipStream);
//
//				target.Seek(0, SeekOrigin.Begin);
//				var serializedGrammar = target.ToArray();
//
//				Console.WriteLine(Convert.ToBase64String(serializedGrammar));
//
//				using (var zipStream = new GZipStream(target, CompressionMode.Decompress, true))
//				{
//					var newGram = ser.DeserializeGrammar(zipStream);
//					Console.WriteLine(newGram);
//				}
//			}

			Console.WriteLine();
//			Console.WriteLine("======== SLR Table ========");
//			using (var target = File.OpenWrite("slr.earle-cache"))
//			{
//				ser.Serialize(slr, target);
//			}
		}

		private static void TestParser(IParser p, string input)
		{
			var result = p.Parse(new Lexer(null, new[] {"a", "b", "d"}).Tokenize(input, "x").ToArray());

			Console.WriteLine(result.ToString() != input
				? $"ERROR: input: {input}, output: {result}"
				: $"OK: {input}");
		}
		
		private static void TestParser(string name, IParser p)
		{
			Console.WriteLine($"==== TESTING {name} ====");
			TestParser(p, "b d a");
			TestParser(p, "b  a");
			TestParser(p, " d a");
			TestParser(p, "  a");
			Console.WriteLine($"==== END TEST {name} ====");
			Console.WriteLine();
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
			var delayF = delay.Type == EarleValueType.NumberFloat ? delay.FloatValue : delay.IntValue;

			while (sw.Elapsed.TotalSeconds < delayF)
				yield return null;
		}

		private static void Run(string file, string function, bool ignorePrint, params string[] precompiledFiles)
		{
			var sw = new Stopwatch();

			sw.Start();
			var interpreter = new EarleInterpreter(path => path.IsEmpty
				? null
				: File.ReadAllText($"scripts/{path.RelativePath}.earle"));

			// Register a "print" function to print
			interpreter.Natives.Register("print", 1, a =>
			{
				if (!ignorePrint)
					Console.WriteLine(a[0].Convert(EarleValueType.String).StringValue);
				
				return EarleValue.Null;
			});
			
			// Register a "waittick" function which halts execution for one game tick
			interpreter.Natives.Register("waittick", 0, (Func<EarleValue[], IEnumerator>) WaitOneFrame);
			
			// Register a "wait" function to implement the wait statement
			interpreter.Natives.Register("wait", 1, (Func<EarleValue[], IEnumerator>) Wait);

			// Compile main files
			sw.Stop();
			var buildInterpreter = sw.Elapsed;

			sw.Restart();
			var mainFn = interpreter[file][function];

			foreach (var precompile in precompiledFiles)
			{
				var precompiledFile = interpreter.GetFile(precompile);
				if (precompiledFile == null)
				{
					throw new Exception($"Could not precompile {precompile}");
				}
			}

			sw.Stop();
			var compiling = sw.Elapsed;

			// Invoke the function
			sw.Restart();
			var ok = interpreter.Invoke(mainFn, EarleValue.Null, out var res);

			// mock update loop
			if (!ok)
			{
				while (interpreter.Tick())
				{
					Thread.Sleep(50);
				}
			}
			
			sw.Stop();
			var running = sw.Elapsed;

			Console.WriteLine($"======== Executed {file}::{function} ========");
			Console.WriteLine($"Initial result: OK: {ok}, result: {res}");
			Console.WriteLine("Building interpreter: " + buildInterpreter);
			Console.WriteLine("Compiling: " + compiling);
			Console.WriteLine("Running: " + running);
		}
	}
}