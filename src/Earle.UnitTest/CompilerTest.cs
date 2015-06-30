using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Earle.Variables;
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
        public void CompilerTestReference()
        {
            AssertResult(200, @"init() { a=200; add(a); return a; } add(v){v++;}");
        }
    }
}
