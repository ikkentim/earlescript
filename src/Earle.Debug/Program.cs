// Earle
// Copyright 2015 Tim Potze
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

namespace Earle.Debug
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var code = @"
                cprint(inp) {
                    print(inp);
                }
                entry () {
                    \External\Directory::Count(234);
                    cprint(""Hello World!"");
                }
                ";

            var code2 = @"
                Count(num) {
                    print(""Count was called!"");
                }
                ";
            var engine = new Engine(path =>
            {
                switch (path)
                {
                    case "\\main":
                        return GenerateStreamFromString(code);
                    case "\\External\\Directory":
                        return GenerateStreamFromString(code2);
                    default:
                        return null;
                }
            });


            Console.WriteLine("Running code: \n {0} \n \n Result:\n", code);

            engine["\\main"].Run();

            Console.ReadLine();
        }

        private static Stream GenerateStreamFromString(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
    }
}