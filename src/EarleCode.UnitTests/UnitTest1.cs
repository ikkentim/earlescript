using System;
using System.Linq;
using EarleCode.Values;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EarleCode.UnitTests
{
    [TestClass]
    public class RuntimeTests
    {
        private void TestCode<T>(string script, T expectedValue, params object[] args)
        {
            var runtime = new Runtime();
            var result = runtime.CompileFile("\\test", $"run(){{{script}}}")
                .Invoke("run", args.Select(EarleValueUtility.ToEarleValue).ToArray());

            Assert.IsNotNull(result);
            Assert.AreEqual(true, result.HasValue);

            if (expectedValue != null)
                Assert.AreEqual(typeof (T), result?.Value?.GetType());

            Assert.AreEqual(expectedValue, (T) result.Value.Value);
        }

        [TestMethod]
        public void TestReturn()
        {
            TestCode("return 41;", 41);
        }

        [TestMethod]
        public void TestAssignment()
        {
            TestCode("i=42; return i;", 42);
        }

        [TestMethod]
        public void TestOrderOfOperations()
        {
            TestCode("return 12 + 3 - 34*57 - 71*9 - 1*5 + 33;", 12 + 3 - 34 * 57 - 71 * 9 - 1 * 5 + 33);
        }
        [TestMethod]
        public void TestAdd()
        {
            TestCode("return 234+123;", 234 + 123);
            TestCode("return 234+123.0;", 234 + 123.0f);
        }
    }
}
