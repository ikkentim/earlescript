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


using EarleCode.Compiler.Lexing;
using EarleCode.Runtime.Instructions;

namespace EarleCode.Compiler.Parsers
{
    public class StatementForParser : Parser
    {
        #region Overrides of Parser

        protected override void Parse()
        {
            // Output:
            // PUSH_S       (1)
            // ASSIGNMENT   (?)
            // EXPRESSION   (checkLength)
            // JUMP_FALSE N (5)
            // CODE_BLOCK   (block.Length)
            // ASSIGNMENT   (incrementBlock.Length)
            // JUMP N       (5)
            // POP_S        (1)

            int checkLength;
            var incrementBlock = CompiledBlock.Empty;

            Yield(OpCode.PushScope);

            Lexer.SkipToken(TokenType.Identifier, "for");
            Lexer.SkipToken(TokenType.Token, "(");

            if (SyntaxMatches("ASSIGNMENT"))
                Parse<StatementAssignmentParser>();
            Lexer.SkipToken(TokenType.Token, ";");

            if (Lexer.Current.Is(TokenType.Token, ";"))
            {
                PushInteger(1);
                checkLength = 5;
            }
            else
            {
                checkLength = Parse<ExpressionParser>();
            }
            Lexer.SkipToken(TokenType.Token, ";");


            if (!Lexer.Current.Is(TokenType.Token, ")"))
                incrementBlock = ParseToBuffer<StatementAssignmentParser>();
            Lexer.SkipToken(TokenType.Token, ")");

            // todo: break and continue
            var block = CompileBlock(true, true);

            PushJump(false, block.Length + 5 + incrementBlock.Length);

            Yield(block, true, incrementBlock.Length + 5, true, 0);

            Yield(incrementBlock);

            PushJump(-5 - block.Length - incrementBlock.Length - 5 - checkLength);


            Yield(OpCode.PopScope);
        }

        #endregion
    }
}