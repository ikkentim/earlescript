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

namespace EarleCode.Retry.Debug
{
    public class Program
    {
        private static void Main(string[] args)
        {
            var runtime = new Runtime();


            runtime.AddFile(runtime.CompileFile("\\file",
                "bravo(value1, value2) {" +
                "   print(value1 + \" equals \" + value2);" +
                "}\n"));


            runtime.AddFile(runtime.CompileFile("\\file2",
                "test(v){" +
                "   if(v){" +
                "       print(\"v has a value!\");" +
                "   }" +
                "}\n"));

            runtime.AddFile(runtime.CompileFile("\\main",
                "init() {\n" +
                "    \\file2::test(null);\n" +
                "    alpha = 5 - -4 * 5 + 20;\n" +
                "    \\file::bravo(\"alpha\", alpha);\n" +
                "   while(alpha){" +
                "       print(\"infinite \" + alpha);" +
                "       alpha = alpha + 1;" +
                "   }" +
                "}"));
            
            runtime.Invoke(runtime.GetFile("\\main").GetFunction("init"));


            Console.ReadLine();
        }
    }
}