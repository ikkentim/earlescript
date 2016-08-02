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

using System;
using System.Linq;
using EarleCode.Compiler.Lexing;
using EarleCode.Runtime;
using EarleCode.Runtime.Instructions;

namespace EarleCode.Compiler.Parsers
{
    internal class AssignmentExpressionParser : Parser
    {
        private OpCode GetOperator(OperatorType type)
        {
            var str = "";
            do
            {
                str += Lexer.Current.Value;
                Lexer.AssertMoveNext();
            } while (Lexer.Current.Is(TokenType.Token) && EarleOperators.GetMaxOperatorLength(type, str) > str.Length);

            return EarleOperators.GetOpCode(type, str);
        }

        protected virtual void YieldDuplicate()
        {
            Yield(OpCode.Duplicate);
        }

        #region Overrides of Parser

        protected override bool RequiresScope => true;

        protected override void Parse()
        {
            // Output:
            // ?

            OpCode modOperator = OpCode.Nop;
            var unaryModOperatorIsPrefix = true;

            if (SyntaxMatches("OPERATOR_MOD_UNARY"))
            {
                modOperator = GetOperator(OperatorType.AssignmentModOperator);
            }

            Lexer.AssertToken(TokenType.Identifier);

            var name = Lexer.Current.Value;
            Lexer.AssertMoveNext();

            var derefBuffer = ParseToBuffer<DereferenceParser>();

            if (modOperator == OpCode.Nop && SyntaxMatches("OPERATOR_MOD_UNARY"))
            {
                modOperator = GetOperator(OperatorType.AssignmentModOperator);
                unaryModOperatorIsPrefix = false;
            }

            if (modOperator != OpCode.Nop)
            {
                // Read
                PushReference(null, name);
                Yield(derefBuffer);

                Yield(OpCode.Read);
                
                // Postfix duplicate
                if (!unaryModOperatorIsPrefix)
                    YieldDuplicate();

                // Operator call
                Yield(OpCode.PushOne);
                Yield(modOperator);

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
                OpCode unaryOperator = OpCode.Nop;
                if (SyntaxMatches("OPERATOR_UNARY"))
                    unaryOperator = GetOperator(OperatorType.AssignmentOperator);

                Lexer.SkipToken(TokenType.Token, "=");

                if (unaryOperator != OpCode.Nop)
                {
                    PushReference(null, name);
                    Yield(derefBuffer);
                    Yield(OpCode.Read);
                }

                Parse<ExpressionParser>();

                if(unaryOperator != OpCode.Nop)
                    Yield(unaryOperator);

                YieldDuplicate();

                PushReference(null, name);
                Yield(derefBuffer);
                Yield(OpCode.Write);
            }
        }

        #endregion
    }
}