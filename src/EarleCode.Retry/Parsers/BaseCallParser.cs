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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EarleCode.Retry.Instructions;
using EarleCode.Retry.Lexing;

namespace EarleCode.Retry.Parsers
{
    public class BaseCallParser : IParser
    {
        #region Implementation of IParser

        public virtual IEnumerable<byte> Parse(Runtime runtime, Compiler compiler, EarleFile file,
            ILexer lexer)
        {
            string path = null;

            // object variableNameExpression = null;

            if (!compiler.SyntaxGrammarProcessor.Matches(lexer, "FUNCTION_CALL_PART"))
            {
                // a target is supplied
                throw new NotImplementedException();
            }

            // optional token to specify function is part of current file
            if (lexer.Current.Is(TokenType.Token, "::"))
            {
                lexer.SkipToken(TokenType.Token, "::");
            }
            // a specific path is supplied
            else if (lexer.Current.Is(TokenType.Token, "\\"))
            {
                // Construct path to the function
                var identifier = false;
                path = "";
                do
                {
                    // check syntax
                    if (identifier)
                        lexer.AssertToken(TokenType.Identifier);
                    else
                        lexer.AssertToken(TokenType.Token, "\\");
                    identifier = !identifier;

                    path += lexer.Current.Value;

                    lexer.AssertMoveNext();
                } while (!lexer.Current.Is(TokenType.Token, "::"));

                lexer.SkipToken(TokenType.Token, "::");
            }
            
            lexer.AssertToken(TokenType.Identifier);
            var name = lexer.Current.Value;
            lexer.AssertMoveNext();
            
            lexer.SkipToken(TokenType.Token, "(");

            // todo: arguments
            yield return (byte)OpCode.PushString;
            foreach (var c in "test")
                yield return (byte) c;
            yield return 0;

            lexer.SkipToken(TokenType.Token, ")");

            yield return (byte) OpCode.PushReference;
            
            foreach (var b in $"{path}::{name}".Select(c => (byte)c))
                yield return b;
            yield return 0;

            yield return (byte)OpCode.Call;
        }

        #endregion
    }
}