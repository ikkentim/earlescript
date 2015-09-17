// EarleCode
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

using System.Collections.Generic;
using EarleCode.Blocks;
using EarleCode.Tokens;

namespace EarleCode.Parsers
{
    public class AssignmentExpressionParser : Parser<AssignmentExpression>
    {
        #region Overrides of Parser<AssignmentExpression>

        public override AssignmentExpression Parse(ICompiler compiler, IScriptScope scriptScope, ITokenizer tokenizer)
        {
            tokenizer.AssertToken(TokenType.Identifier);

            var name = tokenizer.Current.Value;
            var indexers = new List<IExpression>();

            tokenizer.AssertMoveNext();

            var expressionParser = new ExpressionParser();
            while (tokenizer.Current.Is(TokenType.Token, "["))
            {
                indexers.Add(expressionParser.Parse(compiler, scriptScope, tokenizer));
                tokenizer.SkipToken(TokenType.Token, "]");
            }

            tokenizer.SkipToken(TokenType.Token, "=");

            var expression = expressionParser.Parse(compiler, scriptScope, tokenizer);
            return new AssignmentExpression(scriptScope, name, indexers.ToArray(), expression);
        }

        #endregion
    }
    public class AssignmentUnaryExpressionParser : Parser<AssignmentUnaryExpression>
    {
        #region Overrides of Parser<AssignmentExpression>

        public override AssignmentUnaryExpression Parse(ICompiler compiler, IScriptScope scriptScope, ITokenizer tokenizer)
        {
            var isPostOperation = true;
            string operatorToken = null;

            if (tokenizer.Current.Is(TokenType.Token, "--", "++"))
            {
                isPostOperation = false;
                operatorToken = tokenizer.Current.Value;
                tokenizer.AssertMoveNext();
            }

            tokenizer.AssertToken(TokenType.Identifier);

            var name = tokenizer.Current.Value;
            var indexers = new List<IExpression>();

            tokenizer.AssertMoveNext();

            var expressionParser = new ExpressionParser();
            while (tokenizer.Current.Is(TokenType.Token, "["))
            {
                indexers.Add(expressionParser.Parse(compiler, scriptScope, tokenizer));
                tokenizer.SkipToken(TokenType.Token, "]");
            }

            if (isPostOperation)
            {
                tokenizer.AssertToken(TokenType.Token, "--", "++");
                operatorToken = tokenizer.Current.Value;
                tokenizer.AssertMoveNext();
            }
            return new AssignmentUnaryExpression(scriptScope, name, indexers.ToArray(), operatorToken, isPostOperation);
        }

        #endregion
    }
}