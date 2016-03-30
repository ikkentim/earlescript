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

using System.Linq;
using EarleCode.Instructions;
using EarleCode.Lexing;

namespace EarleCode.Parsers
{
    public class AssignmentExpressionParser : Parser
    {
        private string GetUnaryOperators(string[] opList)
        {
            var str = "";
            do
            {
                str += Lexer.Current.Value;
                Lexer.AssertMoveNext();
            } while (Lexer.Current.Is(TokenType.Token) && opList.Any(o => o.StartsWith(str)) &&
                     opList.Where(o => o.StartsWith(str)).Max(o => o.Length) > str.Length);

            return str;
        }
        #region Overrides of Parser

        protected override void Parse()
        {
            // NAME = EXPRESSION
            // OPERATOR_MOD_UNARY NAME
            // NAME OPERATOR_MOD_UNARY

            string unaryModOperator = null;
            var unaryModOperatorIsPrefix = true;

            if (SyntaxMatches("OPERATOR_MOD_UNARY"))
            {
                unaryModOperator = GetUnaryOperators(EarleOperators.UnaryAssignmentModOperators);
            }

            Lexer.AssertToken(TokenType.Identifier);

            var name = Lexer.Current.Value;
            Lexer.AssertMoveNext();

            if (unaryModOperator == null && SyntaxMatches("OPERATOR_MOD_UNARY"))
            {
                unaryModOperator = GetUnaryOperators(EarleOperators.UnaryAssignmentModOperators);
                unaryModOperatorIsPrefix = false;
            }

            if (unaryModOperator != null)
            {
                // Read
                PushReference(null, name);
                Yield(OpCode.Read);

                // Postfix duplicate
                if (!unaryModOperatorIsPrefix)
                    Yield(OpCode.Duplicate);

                // Operator call
                PushReference(null, $"operator{unaryModOperator}");
                Yield(OpCode.Call);
                Yield(1);

                // Prefix dupelicate
                if (unaryModOperatorIsPrefix)
                    Yield(OpCode.Duplicate);

                // Write
                PushReference(null, name);
                Yield(OpCode.Write);
            }
            else
            {
                string unaryOperator = null;
                if (SyntaxMatches("OPERATOR_UNARY"))
                    unaryOperator = GetUnaryOperators(EarleOperators.UnaryAssignmentOperators);

                Lexer.SkipToken(TokenType.Token, "=");

                if (unaryOperator != null)
                {
                    PushReference(null, name);
                    Yield(OpCode.Read);
                }

                Parse<ExpressionParser>();

                if (unaryOperator != null)
                {
                    PushReference(null, $"operator{unaryOperator}");
                    Yield(OpCode.Call);
                    Yield(2);
                }

                Yield(OpCode.Duplicate);
                PushReference(null, name);
                Yield(OpCode.Write);
            }
        }

        #endregion
    }
}