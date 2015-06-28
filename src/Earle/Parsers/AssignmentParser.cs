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

using System.Linq;
using Earle.Blocks;
using Earle.Blocks.Expressions;
using Earle.Tokens;
using Earle.Variables;

namespace Earle.Parsers
{
    public class AssignmentParser : Parser<Assignment>
    {
        private readonly ExpressionParser _expressionParser = new ExpressionParser();

        public AssignmentParser()
            : base("ASSIGNMENT")
        {
        }

        #region Overrides of Parser<Assignment>

        public override Assignment Parse(Block parent, Tokenizer tokenizer)
        {
            AssertToken(tokenizer, TokenType.Identifier);

            var name = tokenizer.Current.Value;

            MoveNext(tokenizer);

            if (Compiler.Grammar.Matches(tokenizer, "OPERATOR_POST_UNARY"))
            {
                var optokens = tokenizer.Current.Value;
                var optoken = optokens.First().ToString();
                SkipToken(tokenizer, optokens, TokenType.Token);

                return new Assignment(parent, name,
                    new OperatorExpression(parent, new VariableExpression(parent, name), optoken,
                        new ValueExpression(parent, new ValueContainer(VarType.Integer, 1))));
            }
            SkipToken(tokenizer, "=", TokenType.Token);

            var result = new Assignment(parent, name, _expressionParser.Parse(parent, tokenizer));

            return result;
        }

        #endregion
    }
}