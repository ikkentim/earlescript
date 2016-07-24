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
using System.IO;
using EarleCode.Localization;
using EarleCode.Values;
using System.Diagnostics;
using System.Threading;

namespace EarleCode.Debug
{
    public class Measure : IDisposable
    {
        private readonly Stopwatch sw;
        public Measure()
        {
            sw = new Stopwatch();
            sw.Start();
        }
        public void Dispose()
        {
            sw.Stop();
            Console.WriteLine($"Took: {sw.Elapsed}");
        }
    }

    public class Program
    {
        private static string GetRelativePath(string filespec, string folder)
        {
            var pathUri = new Uri(filespec);

            if(!folder.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal))
                folder += Path.DirectorySeparatorChar;
            
            var folderUri = new Uri(folder);
            return Uri.UnescapeDataString(folderUri.MakeRelativeUri(pathUri).ToString().Replace('/', Path.DirectorySeparatorChar));
        }

        private static void Main(string[] args)
        {
            var runtime = new Runtime();
            
            var codeDir = Path.Combine(Directory.GetCurrentDirectory(), "code");

            foreach (var file in Directory.GetFiles(codeDir, "*.earle", SearchOption.AllDirectories))
            {
                var rel = GetRelativePath(file, codeDir).Replace('/', '\\');
                rel = '\\' + rel.Substring(0, rel.Length - 6);
                runtime.CompileFile(rel, File.ReadAllText(file));
            }
            
            var loc = new Localizer();
            foreach (var file in Directory.GetFiles(codeDir, "*.estr", SearchOption.AllDirectories))
            {
                var rel = GetRelativePath(file, codeDir).Replace('/', '\\');
                rel = rel.Substring(0, rel.Length - 5);
                loc.LoadFromFile(rel, File.ReadAllText(file), Path.GetFileNameWithoutExtension(file).ToUpper() + "_");
            }
            loc.AddToRuntime(runtime);
            loc.Key = "LANG_ENGLISH";

            var result = runtime.GetFile("\\main").Invoke("init", EarleValue.Undefined);

            Console.WriteLine();
            Console.WriteLine("Code execution completed!");
            Console.WriteLine("Result: " + result);

            if (result?.Is<EarleStructure>() ?? false)
            {
                var struc = result.Value.As<EarleStructure>();
                foreach(var kv in struc)
                    Console.WriteLine($"> {kv.Key} = {kv.Value}");
            }

            if (result?.Is<EarleArray>() ?? false)
            {
                var arr = result.Value.As<EarleArray>();
                foreach (var v in arr)
                    Console.WriteLine(v);
            }

            do
            {
                Thread.Sleep(1000 / 30);

            } while(!runtime.Tick());

            Console.WriteLine("Done!");
//            Console.ReadLine();
        }
    }
}