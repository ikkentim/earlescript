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
using EarleCode.Instructions;
using EarleCode.Lexing;

namespace EarleCode.Parsers
{
    public class CallExpressionParser : Parser
    {
        #region Overrides of Parser

        protected override void Parse()
        {
            // Output:
            // TARGET?      (?)
            // ARGUMENTS    (?)
            // REFERENCE    (?)
            // CALL(_T) N   (5)

            bool hasTarget = !SyntaxMatches("FUNCTION_CALL_PART");
            if(!SyntaxMatches("FUNCTION_CALL_PART"))
                Parse<FunctionTargetExpressionParser>();
            
            while(SyntaxMatches("FUNCTION_CALL_PART"))
            {
                var referenceBuffer = ParseToBuffer<FunctionReferenceExpressionParser>();

                Lexer.SkipToken(TokenType.Token, "(");

                var arguments = 0;
                while(!Lexer.Current.Is(TokenType.Token, ")"))
                {
                    Parse<ExpressionParser>();
                    arguments++;

                    if(Lexer.Current.Is(TokenType.Token, ")"))
                        break;

                    Lexer.SkipToken(TokenType.Token, ",");
                }

                Lexer.SkipToken(TokenType.Token, ")");
                Yield(referenceBuffer);
                if(hasTarget)
                    PushCall(arguments);
                else
                    PushCallWithoutTarget(arguments);
            }
        }

        #endregion
    }
}