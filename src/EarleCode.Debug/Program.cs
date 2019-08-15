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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using CommandLine;
using EarleCode.Compiling.Lexing;
using EarleCode.Compiling.Parsing;
using EarleCode.Compiling.Parsing.Grammars;
using EarleCode.Compiling.Parsing.Grammars.Productions;
using EarleCode.Compiling.Parsing.ParseTree;
using EarleCode.Interpreter;

namespace EarleCode.Debug
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			
			var gg = new EnumGrammar<TestGram>(null);
			var pt = new CanonicalParsingTable(gg);
			
			Console.WriteLine("terms: " + string.Join(", ", gg.Terminals));
			Console.WriteLine("nonterms: " + string.Join(", ", gg.NonTerminals));

			Console.WriteLine("SLR Sets:");
			var canonicalCollection2 = new SLRCanonicalCollection(gg);
			var sets2 = canonicalCollection2.Sets.ToArray();

			var k = 0;
			foreach (var s in sets2)
			{
				Console.WriteLine($"I{k++}: {s}");
			}


			Console.WriteLine();

			Console.WriteLine("LALR Sets:");
			var canonicalCollection = new LALRCanonicalCollection(gg);
			var sets = canonicalCollection.Sets.ToArray();

			var j = 0;
			foreach (var s in sets)
			{
				Console.WriteLine($"I{j++}: {s}");
			}


			Console.WriteLine();

			for (var i = 0; i < pt.States; i++)
			{
				foreach (var n in gg.NonTerminals)
				{
					if (pt.TryGetGoTo(i, n, out var r))
						Console.WriteLine($"[{i}, {n}] = GOTO {r}");
				}

				foreach (var t in gg.Terminals)
				{
					if (pt.TryGetAction(i, t, out var r))
						Console.WriteLine($"[{i}, {t}] = {r}");
				}
			}


			var newTable = new LALRParsingTableBuilder().Build(gg);

			Console.WriteLine("LALR:");

			for (var i = 0; i < newTable.States; i++)
			{
				foreach (var n in gg.NonTerminals)
				{
					if (newTable.TryGetGoTo(i, n, out var r))
						Console.WriteLine($"[{i}, {n}] = GOTO {r}");
				}

				foreach (var t in gg.Terminals)
				{
					if (newTable.TryGetAction(i, t, out var r))
						Console.WriteLine($"[{i}, {t}] = {r}");
				}
			}

			var parserX = new ShiftReduceParser(newTable);

			var result =parserX.Parse(new Lexer(null, new[] {"c", "d"}).Tokenize("c c c d d", "foo").ToArray());
			Console.WriteLine(result.ToTreeString());

			return;// */

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

				foreach(var r in g.All)
					Console.WriteLine(r);
				Console.WriteLine();
				var col = new SLRCanonicalCollection(g);
				foreach(var s in col.Sets)
				{
					Console.WriteLine("CANONICAL COLLECTION: " + s);
					foreach (var x in col[s].Keys)
					{
						Console.WriteLine(x + " :: " + col[s][x]);
					}
				}
				TestParser("LL1", new LL1Parser(g));
				TestParser("SLR", new SLRParser(g));
				//TestParser("LALR", new LALRParser(g)); // FIXME: LALR Parser hardcoded to earle at the moment

				return;
			}

			Run(opts?.InputFile ?? "\\main", opts?.Function ?? "main", opts?.IgnorePrint ?? false);
		}

		private static void Serialization()
		{
			throw new NotImplementedException();
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

			//*
			Console.WriteLine($"======== Executed {file}::{function} ========");
			Console.WriteLine($"Initial result: OK: {ok}, result: {res}");
			Console.WriteLine("Building interpreter: " + buildInterpreter);
			Console.WriteLine("Compiling: " + compiling);
			Console.WriteLine("Running: " + running);
			// */
		}
	}

	public enum TestGram
	{
		[Rule("S")] St,
		[Rule("C C")] S,
		[Rule("`c` C", "`d`")] C
	}
}