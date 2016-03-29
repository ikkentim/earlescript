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
            string path = null;

            if (!SyntaxMatches("FUNCTION_CALL_PART"))
            {
                // a target is supplied
                throw new NotImplementedException();
            }

            // optional token to specify function is part of current file
            if (Lexer.Current.Is(TokenType.Token, ":"))
            {
                Lexer.SkipToken(TokenType.Token, ":");
                Lexer.SkipToken(TokenType.Token, ":");
            }
            // a specific path is supplied
            else if (Lexer.Current.Is(TokenType.Token, "\\"))
            {
                // Construct path to the function
                var identifier = false;
                path = "";
                do
                {
                    // check syntax
                    if (identifier)
                        Lexer.AssertToken(TokenType.Identifier);
                    else
                        Lexer.AssertToken(TokenType.Token, "\\");
                    identifier = !identifier;

                    path += Lexer.Current.Value;

                    Lexer.AssertMoveNext();
                } while (!Lexer.Current.Is(TokenType.Token, ":"));

                Lexer.SkipToken(TokenType.Token, ":");
                Lexer.SkipToken(TokenType.Token, ":");
            }

            Lexer.AssertToken(TokenType.Identifier);
            var name = Lexer.Current.Value;
            Lexer.AssertMoveNext();

            Lexer.SkipToken(TokenType.Token, "(");

            var arguments = 0;
            while (!Lexer.Current.Is(TokenType.Token, ")"))
            {
                Parse<ExpressionParser>();
                arguments++;

                if (Lexer.Current.Is(TokenType.Token, ")"))
                    break;

                Lexer.SkipToken(TokenType.Token, ",");
            }

            Lexer.SkipToken(TokenType.Token, ")");

            PushReference(path, name);

            Yield(OpCode.Call);
            Yield(arguments);
        }

        #endregion
    }
}