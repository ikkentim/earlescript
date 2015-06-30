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
        private void AssertExpressionType(string expected, string expression, int tokens)
        {
            var t = new Tokenizer("test", expression);
            t.MoveNext();
            int tc;
            Assert.AreEqual(expected, Compiler.Grammar.GetMatch(t, out tc),
                string.Format("`{0}` should be of type {1}", expression, expected));

            Assert.AreEqual(tokens, tc, "Unexpected token count in `" + expression + "`");
        }

        private void AssertFunctionExpressionType(string expected, string expression, int tokens)
        {
            var t = new Tokenizer("test", expression);
            t.MoveNext();
            int tc;
            Assert.AreEqual(expected, Compiler.FunctionGrammar.GetMatch(t, out tc),
                string.Format("`{0}` should be of type {1}", expression, expected));

            Assert.AreEqual(tokens, tc, "Unexpected token count in `" + expression + "`");
        }

        [TestMethod]
        public void GrammarTestFunction()
        {
            AssertFunctionExpressionType("FUNCTION_DECLARATION", "func()",3);
            AssertFunctionExpressionType("FUNCTION_DECLARATION", "func(a,b)", 6);
            AssertFunctionExpressionType("FUNCTION_DECLARATION", "func(a)", 4);
        }

        [TestMethod]
        public void GrammarTestReturn()
        {
            AssertExpressionType("STATEMENT_RETURN", @"return;", 2);
            AssertExpressionType("STATEMENT_RETURN", @"return "" Hello world! "";",3);
            AssertExpressionType("STATEMENT_RETURN", @"return 1;",3);
            AssertExpressionType("STATEMENT_RETURN", @"return 1 + 1;",5);
        }

        [TestMethod]
        public void GrammarTestExpression()
        {
            AssertExpressionType("EXPRESSION", @"test_call", 1);
            AssertExpressionType("EXPRESSION", @"""awesome""", 1);
            AssertExpressionType("EXPRESSION", @"123", 1);
            AssertExpressionType("EXPRESSION", "-123", 2);
            AssertExpressionType("EXPRESSION", "123 - 12", 3);
            AssertExpressionType("EXPRESSION", "123 * 12", 3);
            AssertExpressionType("EXPRESSION", "123 + 12", 3);
        }

        [TestMethod]
        public void GrammarTestIndexedExpression()
        {
            AssertExpressionType("EXPRESSION", @"array[""index""]", 4);
            AssertExpressionType("EXPRESSION", @"array[array[""2ndindex""]]", 7);
            AssertExpressionType("EXPRESSION", @"array[""index""][""2ndindex""]", 7);
            AssertExpressionType("EXPRESSION", @"array[8]", 4);
            AssertExpressionType("EXPRESSION", @"array[index]", 4);
            AssertExpressionType("EXPRESSION", @"""value: "" + array[""index""]", 6);
        }

        [TestMethod]
        public void GrammarTestFunctionCall()
        {
            AssertExpressionType("FUNCTION_CALL", "some_function()", 3);
            AssertExpressionType("FUNCTION_CALL", @"some_function(123)", 4);
            AssertExpressionType("FUNCTION_CALL", @"some_function(123, ""abc"")", 6);
            AssertExpressionType("FUNCTION_CALL", @"some_function(123 * 123)", 6);
            AssertExpressionType("FUNCTION_CALL", @"test_call3 (a,b,c,123,""awesome"")", 12);
            AssertExpressionType("FUNCTION_CALL", @"test_call2 (awesomeness)", 4);

            AssertExpressionType("FUNCTION_CALL", @"cprint(""Hello "" + ""World! ("" + sum + "")"")", 10);

            AssertExpressionType("FUNCTION_CALL", @"test_call (arr[1])", 7);
        }

        [TestMethod]
        public void GrammarTestAssignment()
        {
            AssertExpressionType("ASSIGNMENT", @"alpha = bravo;", 3);
        }

        [TestMethod]
        public void GrammarTestStatementIf()
        {
            AssertExpressionType("STATEMENT_IF", @"if (2 > 3)", 6);
            AssertExpressionType("STATEMENT_IF", @"if (2 + 39 / 2 > 3)", 10);
        }

        [TestMethod]
        public void GrammarTestOperator()
        {
            AssertExpressionType("OPERATOR", @"-", 1);
            AssertExpressionType("OPERATOR", @"<", 1);
            AssertExpressionType("OPERATOR", @"<<", 1);
        }
    }
}