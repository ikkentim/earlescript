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
using System.Text.RegularExpressions;

namespace Earle.Tokens
{
    public class Tokenizer : IEnumerator<Token>
    {
        private readonly string _input;
        private readonly Stack<Token> _pushedTokens = new Stack<Token>();
        private readonly List<TokenData> _tokenDatas = new List<TokenData>();
        private readonly char[] _whitespace = {' ', '\t', '\r', '\n'};
        private int _position;
        private int _previousPosition;

        public Tokenizer(string input)
        {
            if (input == null) throw new ArgumentNullException("input");
            _input = input;

            _tokenDatas.Add(new TokenData(@"^[a-zA-Z_][a-zA-Z0-9_]*", TokenType.Identifier));
            _tokenDatas.Add(new TokenData(@"^[0-9]*\.?[0-9]+", TokenType.NumberLiteral));
            _tokenDatas.Add(new TokenData(@"^([""])((?:\\\1|.)*?)\1", TokenType.StringLiteral, 2));

            foreach (
                var s in
                    new[]
                    {
                        "+=", "-=", "*=", "/=", "%=", "&=", "|=", "^=", "<<=", ">>=", "++", "--", "<<", ">>", "==",
                        "!=", "<=", ">=", "&&", "||"
                    }.OrderByDescending(s => s.Length))
                _tokenDatas.Add(new TokenData(@"^\" + string.Join(@"\", s.ToCharArray()), TokenType.Token));

            SkipWhitespace();
            Next();
        }

        private string Buffer
        {
            get { return _input.Substring(_position); }
        }

        public string CurrentBuffer
        {
            get { return _input.Substring(_previousPosition); }
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
            while (_position < _input.Length && _whitespace.Contains(_input[_position]))
                _position++;
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
                    _previousPosition = _position;
                    _position += match.Groups[0].Length;
                    Current = new Token(tokenData.Type, match.Groups[tokenData.ContentGroup].Value,
                        _input.Take(_position).Count(c => c == '\n') + 1);

                    SkipWhitespace();

                    return;
                }
            }

            Current = new Token(TokenType.Token, Buffer.Substring(0, 1),
                _input.Take(_position).Count(c => c == '\n') + 1);
            _previousPosition = _position;
            _position++;
            SkipWhitespace();
        }

        public void PushBack(Token token)
        {
            if (Current != null)
                _pushedTokens.Push(Current);

            Current = token;
        }

        private class TokenData
        {
            private readonly string _pattern;

            public TokenData(Regex pattern, TokenType type)
            {
                Pattern = pattern;
                Type = type;
            }

            public TokenData(string pattern, TokenType type, int contentGroup = 0)
                : this(new Regex(pattern), type)
            {
                _pattern = pattern;
                ContentGroup = contentGroup;
            }

            public int ContentGroup { get; private set; }
            public Regex Pattern { get; private set; }
            public TokenType Type { get; private set; }

            #region Overrides of Object

            public override string ToString()
            {
                return string.Format("{0} `{1}`", Type, _pattern);
            }

            #endregion
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