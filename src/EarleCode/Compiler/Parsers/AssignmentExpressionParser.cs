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
using EarleCode.Compiler.Lexing;
using EarleCode.Runtime;
using EarleCode.Runtime.Instructions;

namespace EarleCode.Compiler.Parsers
{
    public class AssignmentExpressionParser : Parser
    {
        private string GetOperator(string[] opList)
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

        protected virtual void YieldDuplicate()
        {
            Yield(OpCode.Duplicate);
        }

        #region Overrides of Parser

        protected override void Parse()
        {
            // Output:
            // ?

            string unaryModOperator = null;
            var unaryModOperatorIsPrefix = true;

            if (SyntaxMatches("OPERATOR_MOD_UNARY"))
            {
                unaryModOperator = GetOperator(EarleOperators.UnaryAssignmentModOperators);
            }

            Lexer.AssertToken(TokenType.Identifier);

            var name = Lexer.Current.Value;
            Lexer.AssertMoveNext();

            var derefBuffer = ParseToBuffer<DereferenceParser>();

            if (unaryModOperator == null && SyntaxMatches("OPERATOR_MOD_UNARY"))
            {
                unaryModOperator = GetOperator(EarleOperators.UnaryAssignmentModOperators);
                unaryModOperatorIsPrefix = false;
            }

            if (unaryModOperator != null)
            {
                if (unaryModOperator.Length == 0)
                    ThrowUnexpectedToken("-OPERATOR-");

                // Read
                PushReference(null, name);
                Yield(derefBuffer);

                Yield(OpCode.Read);
                
                // Postfix duplicate
                if (!unaryModOperatorIsPrefix)
                    YieldDuplicate();

                // Operator call
                PushCallWithoutTarget(null, $"operator{unaryModOperator}", 1);

                // Prefix dupelicate
                if (unaryModOperatorIsPrefix)
                    YieldDuplicate();

                // Write
                PushReference(null, name);
                Yield(derefBuffer);
                Yield(OpCode.Write);
            }
            else
            {
                string unaryOperator = null;
                if (SyntaxMatches("OPERATOR_UNARY"))
                    unaryOperator = GetOperator(EarleOperators.UnaryAssignmentOperators);

                Lexer.SkipToken(TokenType.Token, "=");

                if (unaryOperator != null)
                {
                    PushReference(null, name);
                    Yield(derefBuffer);
                    Yield(OpCode.Read);
                }

                Parse<ExpressionParser>();

                if (unaryOperator != null)
                    PushCallWithoutTarget(null, $"operator{unaryOperator}", 2);

                YieldDuplicate();

                PushReference(null, name);
                Yield(derefBuffer);
                Yield(OpCode.Write);
            }
        }

        #endregion
    }
}