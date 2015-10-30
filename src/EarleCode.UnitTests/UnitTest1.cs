using System;
using System.IO;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EarleCode.UnitTests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var runtime = new Runtime();

            runtime.LoadFile(@"\main", @"
init()
{
  println(""test"");
}
");

            //runtime.Invoke(null, "init", @"\main");
        }
    }
}
