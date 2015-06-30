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
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Earle.Tokens
{
    public class Tokenizer : IEnumerator<Token>
    {
        private readonly string _file;
        private readonly string _input;
        private readonly Stack<Token> _pushedTokens = new Stack<Token>();
        private readonly List<TokenData> _tokenDatas = new List<TokenData>();
        private int _column = 1;
        private int _line = 1;
        private int _position;

        public Tokenizer(string file, string input)
        {
            if (file == null) throw new ArgumentNullException("file");
            if (input == null) throw new ArgumentNullException("input");
            _file = file;
            _input = input;

            // Load token data.
            _tokenDatas.Add(new TokenData(@"^[a-zA-Z_][a-zA-Z0-9_]*", TokenType.Identifier));
            _tokenDatas.Add(new TokenData(@"^[0-9]*\.?[0-9]+", TokenType.NumberLiteral));
            _tokenDatas.Add(new TokenData(@"^([""])((?:\\\1|.)*?)\1", TokenType.StringLiteral, 2));

            foreach (
                var s in
                    new[] {"++", "--", "<<", ">>", "==", "!=", "<=", ">=", "&&", "||"}.OrderByDescending(s => s.Length))
                _tokenDatas.Add(new TokenData(@"^\" + string.Join(@"\", s.ToCharArray()), TokenType.Token));

            SkipWhitespace();
            Next();
        }

        private string Buffer
        {
            get { return _input.Substring(_position); }
        }

        #region Implementation of IDisposable

        public void Dispose()
        {
        }

        #endregion

        public bool HasNext()
        {
            return _position < _input.Length || _pushedTokens.Any();
        }

        private void SkipWhitespace()
        {
            while (_position < _input.Length && char.IsWhiteSpace(_input[_position]))
                UpdateLineNumber(_input.Substring(_position, 1));

            if (Buffer.StartsWith("//"))
            {
                var endidx = Buffer.IndexOf("\n", StringComparison.Ordinal);
                UpdateLineNumber(endidx == -1 ? Buffer : Buffer.Substring(0, endidx + 1));
                SkipWhitespace();
            }
            else if (Buffer.StartsWith("/*"))
            {
                var endidx = Buffer.IndexOf("*/", StringComparison.Ordinal);
                UpdateLineNumber(endidx == -1 ? Buffer : Buffer.Substring(0, endidx + 2));
                SkipWhitespace();
            }
        }

        private void UpdateLineNumber(string input)
        {
            foreach (var c in input)
            {
                if (c == '\n')
                {
                    _line++;
                    _column = 1;
                }
                else
                    _column++;
            }

            _position += input.Length;
        }

        private void Next()
        {
            if (_pushedTokens.Any())
            {
                Current = _pushedTokens.Pop();
                return;
            }

            foreach (var tokenData in _tokenDatas)
            {
                var match = tokenData.Pattern.Match(Buffer);

                if (match.Success)
                {
                    Current = new Token(tokenData.Type, match.Groups[tokenData.ContentGroup].Value, _file, _line,
                        _column);

                    UpdateLineNumber(match.Groups[0].Value);
                    SkipWhitespace();
                    return;
                }
            }

            Current = new Token(TokenType.Token, Buffer.Substring(0, 1), _file, _line, _column);
            _position++;
            SkipWhitespace();
        }

        public void PushBack(Token token)
        {
            if (Current != null)
                _pushedTokens.Push(Current);

            Current = token;
        }

        #region Implementation of IEnumerator

        public bool MoveNext()
        {
            if (!HasNext())
            {
                Current = null;
                return false;
            }
            Next();
            return true;
        }

        public void Reset()
        {
            _position = 0;
            _pushedTokens.Clear();

            Next();
        }

        public Token Current { get; private set; }

        object IEnumerator.Current
        {
            get { return Current; }
        }

        #endregion
    }
}