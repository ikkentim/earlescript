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

using Earle.Tokens;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Earle.UnitTest
{
    [TestClass]
    public class GrammarTest
    {
        private void AssertExpressionType(string expected, string expression)
        {
            var t = new Tokenizer("test", expression);

            Assert.AreEqual(expected, Compiler.Grammar.GetMatch(t),
                string.Format("`{0}` should be of type {1}", expression, expected));
        }

        private void AssertFunctionExpressionType(string expected, string expression)
        {
            var t = new Tokenizer("test", expression);

            Assert.AreEqual(expected, Compiler.FunctionGrammar.GetMatch(t),
                string.Format("`{0}` should be of type {1}", expression, expected));
        }

        [TestMethod]
        public void TestFunction()
        {
            AssertFunctionExpressionType("FUNCTION_DECLARATION", "func()");
            AssertFunctionExpressionType("FUNCTION_DECLARATION", "func(a,b)");
            AssertFunctionExpressionType("FUNCTION_DECLARATION", "func(a)");
        }

        [TestMethod]
        public void TestReturn()
        {
            AssertExpressionType("STATEMENT_RETURN", @"return;");
            AssertExpressionType("STATEMENT_RETURN", @"return "" Hello world! "";");
            AssertExpressionType("STATEMENT_RETURN", @"return 1;");
            AssertExpressionType("STATEMENT_RETURN", @"return 1 + 1;");
        }

        [TestMethod]
        public void TestExpression()
        {
            AssertExpressionType("EXPRESSION", "123 - 12");
            AssertExpressionType("EXPRESSION", "123 * 12");
            AssertExpressionType("EXPRESSION", "123 + 12");
            AssertExpressionType("EXPRESSION", "-123");
            AssertExpressionType("EXPRESSION", @"test_call");
            AssertExpressionType("EXPRESSION", @"""awesome""");
            AssertExpressionType("EXPRESSION", @"123");
        }

        [TestMethod]
        public void TestFunctionCall()
        {
            AssertExpressionType("FUNCTION_CALL", "some_function()");
            AssertExpressionType("FUNCTION_CALL", @"some_function(123)");
            AssertExpressionType("FUNCTION_CALL", @"some_function(123, ""abc"")");
            AssertExpressionType("FUNCTION_CALL", @"some_function(123 * 123)");
            AssertExpressionType("FUNCTION_CALL", @"test_call3 (a,b,c,123,""awesome"")");
            AssertExpressionType("FUNCTION_CALL", @"test_call2 (awesomeness)");
            AssertExpressionType("FUNCTION_CALL", @"test_call (1)");
            AssertExpressionType("FUNCTION_CALL", @"cprint(""Hello "" + ""World! ("" + sum + "")"")");
        }

        [TestMethod]
        public void TestAssignment()
        {
            AssertExpressionType("ASSIGNMENT", @"alpha = bravo;");
        }

        [TestMethod]
        public void TestStatementIf()
        {
            AssertExpressionType("STATEMENT_IF", @"if (2 > 3)");
            AssertExpressionType("STATEMENT_IF", @"if (2 + 39 / 2 > 3)");
        }

        [TestMethod]
        public void TestOperator()
        {
            AssertExpressionType("OPERATOR", @"-");
            AssertExpressionType("OPERATOR", @"<");
            AssertExpressionType("OPERATOR", @"<<");
        }
    }
}