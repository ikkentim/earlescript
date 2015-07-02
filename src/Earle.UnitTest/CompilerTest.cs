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

using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Earle.UnitTest
{
    [TestClass]
    public class CompilerTest
    {
        private static Stream GenerateStreamFromString(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        private void AssertResult(object result, string code)
        {
            var earle = new Engine(p => p != "\\main" ? null : GenerateStreamFromString(code));

            var r = earle["\\main"].Run();

            Assert.AreEqual(result, r == null ? null : r.Value);
        }

        [TestMethod]
        public void CompilerTestReturn()
        {
            AssertResult(200, @"init() return 200;");
            AssertResult("OK", @"init() return ""OK"";");
        }

        [TestMethod]
        public void CompilerTestComments()
        {
            AssertResult(200, @"init/*j*/() /*j*/ return 200;");
            AssertResult("OK", @"init() return ""OK"";");
        }

        [TestMethod]
        public void CompilerTestReference()
        {
            AssertResult(200, @"init() { a=200; add(a); return a; } add(v){v++;}");
        }

        [TestMethod]
        public void CompilerTestFunctionCall()
        {
            AssertResult(200, @"init() { return func(); } func(){ return 200; }");
//            AssertResult(200, @"init() { return ::func(); } func(){ return 200; }");
//            AssertResult(200, @"init() { return \main::func(); } func(){ return 200; }");
        }
    }
}