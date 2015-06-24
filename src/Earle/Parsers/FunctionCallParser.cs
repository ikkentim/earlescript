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

using System;
using System.Collections.Generic;
using System.Linq;
using Earle.Blocks;
using Earle.Tokens;
using Earle.Variables;

namespace Earle.Parsers
{
    internal class FunctionCallParser : Parser<FunctionCall>
    {
        private bool IsCurrentToken(Tokenizer tokenizer, Stack<Token> tokens, string token)
        {
            if (IsCurrentToken(tokenizer, token)) return true;

            PushBack(tokenizer, tokens);
            return false;
        }

        private bool IsCurrentToken(Tokenizer tokenizer, Stack<Token> tokens, params TokenType[] token)
        {
            if (IsCurrentToken(tokenizer, token)) return true;

            PushBack(tokenizer, tokens);
            return false;
        }

        private bool IsCurrentToken(Tokenizer tokenizer, string token)
        {
            return tokenizer.Current.Type == TokenType.Token && tokenizer.Current.Value == token;
        }

        private bool IsCurrentToken(Tokenizer tokenizer, params TokenType[] token)
        {
            return token.Contains(tokenizer.Current.Type);
        }

        private bool MoveNext(Tokenizer tokenizer, Stack<Token> tokens)
        {
            var current = tokenizer.Current;

            if (tokenizer.MoveNext())
            {
                tokens.Push(current);
                return true;
            }

            PushBack(tokenizer, tokens);
            return false;
        }

        private void PushBack(Tokenizer tokenizer, Stack<Token> tokens)
        {
            while (tokens.Any())
                tokenizer.PushBack(tokens.Pop());
        }

        #region Overrides of Parser<FunctionCall>

        public override string ParserRule
        {
            get { return "FUNCTION_CALL"; }
        }

        public override FunctionCall Parse(Block parent, Tokenizer tokenizer)
        {
            string path, name;
            if (tokenizer.Current.Type == TokenType.Token && tokenizer.Current.Value == "\\")
            {
                path = string.Empty;
                var seperator = false;
                do
                {
                    if (tokenizer.Current.Type == TokenType.Token)
                    {
                        if (tokenizer.Current.Value == "\\")
                        {
                            if (!seperator)
                                path += "\\";

                            seperator = true;
                        }
                        else
                        {
                            throw new Exception();
                        }
                    }
                    else if (tokenizer.Current.Type == TokenType.Identifier)
                    {
                        path += tokenizer.Current.Value;
                        seperator = false;
                    }
                    else
                    {
                        throw new Exception();
                    }
                } while (tokenizer.MoveNext() &&
                         !(tokenizer.Current.Type == TokenType.Token && tokenizer.Current.Value == ":"));

                tokenizer.MoveNext();

                if (tokenizer.Current.Type != TokenType.Token || tokenizer.Current.Value != ":")
                    throw new Exception();

                tokenizer.MoveNext();

                if (tokenizer.Current.Type != TokenType.Identifier)
                    throw new Exception();

                name = tokenizer.Current.Value;
            }
            else if (tokenizer.Current.Type == TokenType.Identifier)
            {
                path = null;
                name = tokenizer.Current.Value;
            }
            else
            {
                throw new Exception();
            }

            tokenizer.MoveNext();

            if (tokenizer.Current.Type != TokenType.Token || tokenizer.Current.Value != "(")
                throw new Exception();

            tokenizer.MoveNext();

            var arguments = new List<ValueContainer>();
            while (!(tokenizer.Current.Type == TokenType.Token && tokenizer.Current.Value == ")"))
            {
                switch (tokenizer.Current.Type)
                {
                    case TokenType.Identifier:
                        arguments.Add(new ReferencedVariable(parent, tokenizer.Current.Value));
                        break;
                    case TokenType.NumberLiteral:
                        arguments.Add(new ValueContainer(VarType.Number, int.Parse(tokenizer.Current.Value)));
                        break;
                    case TokenType.StringLiteral:
                        arguments.Add(new ValueContainer(VarType.String, tokenizer.Current.Value));
                        break;
                    default:
                        throw new Exception();
                }

                tokenizer.MoveNext();
                if (tokenizer.Current.Type == TokenType.Token && tokenizer.Current.Value == ",")
                    tokenizer.MoveNext();
                else if (!(tokenizer.Current.Type == TokenType.Token && tokenizer.Current.Value == ")"))
                    throw new Exception();
            }

            tokenizer.MoveNext();

            return new FunctionCall(parent, path, name, arguments.ToArray());
        }

        #endregion
    }
}