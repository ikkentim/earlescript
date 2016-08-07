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
    internal class FunctionReferenceExpressionParser : Parser
    {
        #region Overrides of Parser

        protected override void Parse()
        {
            // Output:
            // PUSH_R       (?)

            string path = null;

            // optional token to specify function is part of current file
            if(SyntaxMatches("UNBOX_FUNCTION"))
            {
                Lexer.SkipToken(TokenType.Token, "[");
                Lexer.SkipToken(TokenType.Token, "[");
                var identifier = Lexer.Current.Value;
                Lexer.SkipToken(TokenType.Identifier);
                Lexer.SkipToken(TokenType.Token, "]");
                Lexer.SkipToken(TokenType.Token, "]");

                PushReference(null, identifier);
                Yield(OpCode.UnboxFunctionReference);
            }
            else
            {
                if(Lexer.Current.Is(TokenType.Token, ":"))
                {
                    Lexer.SkipToken(TokenType.Token, ":");
                    Lexer.SkipToken(TokenType.Token, ":");
                }
                // a specific path is supplied
                else if(SyntaxMatches("PATH_PREFIX"))
                {
                    // Construct path to the function
                    var identifier = !Lexer.Current.Is(TokenType.Token, "\\");
                    path = identifier ? "\\" : string.Empty;
                    do
                    {
                        // check syntax
                        if(identifier)
                            Lexer.AssertToken(TokenType.Identifier);
                        else
                            Lexer.AssertToken(TokenType.Token, "\\");
                        identifier = !identifier;

                        path += Lexer.Current.Value;

                        Lexer.AssertMoveNext();
                    } while(!Lexer.Current.Is(TokenType.Token, ":"));

                    Lexer.SkipToken(TokenType.Token, ":");
                    Lexer.SkipToken(TokenType.Token, ":");
                }

                Lexer.AssertToken(TokenType.Identifier);
                var name = Lexer.Current.Value;

                Lexer.AssertMoveNext();

                PushFunctionReference(path, name);
            }
        }

        #endregion
    }
}