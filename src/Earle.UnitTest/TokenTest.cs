using System;
using System.Text;
using System.Collections.Generic;
using Earle.Tokens;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Earle.UnitTest
{
    /// <summary>
    /// Summary description for TokenTest
    /// </summary>
    [TestClass]
    public class TokenTest
    {
        [TestMethod]
        public void TokenTestToUpperString()
        {
            Assert.AreEqual("TOKEN", TokenType.Token.ToUpperString());
            Assert.AreEqual("IDENTIFIER", TokenType.Identifier.ToUpperString());
            Assert.AreEqual("NUMBER_LITERAL", TokenType.NumberLiteral.ToUpperString());
            Assert.AreEqual("STRING_LITERAL", TokenType.StringLiteral.ToUpperString());
        }
    }
}
