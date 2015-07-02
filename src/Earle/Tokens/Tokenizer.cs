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
    /// <summary>
    ///     Represents the lexer.
    /// </summary>
    public class Tokenizer : IEnumerator<Token>
    {
        private static readonly TokenTypeData[] TokenTypes;
        private readonly string _file;
        private readonly string _input;
        private readonly Stack<Token> _pushedTokens = new Stack<Token>();
        private int _caretPosition; // Indicates the caret position.
        private int _column = 1; // Indicates the column number at the caret.
        private int _line = 1; // Indicates the line number at the caret.

        /// <summary>
        ///     Initializes the <see cref="Tokenizer" /> class.
        /// </summary>
        static Tokenizer()
        {
            // Fill the token types array.
            var data = new List<TokenTypeData>
            {
                new TokenTypeData(@"\G[a-zA-Z_][a-zA-Z0-9_]*", TokenType.Identifier),
                new TokenTypeData(@"\G[0-9]*\.?[0-9]+", TokenType.NumberLiteral),
                new TokenTypeData(@"\G([""])((?:\\\1|.)*?)\1", TokenType.StringLiteral, 2)
            };

            data.AddRange(
                new[] {"++", "--", "<<", ">>", "==", "!=", "<=", ">=", "&&", "||", "::"}.Select(
                    s => new TokenTypeData(@"\G\" + string.Join(@"\", s.ToCharArray()), TokenType.Token)));

            TokenTypes = data.ToArray();
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Tokenizer" /> class.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="input">The input.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if file or input is null.</exception>
        public Tokenizer(string file, string input)
        {
            if (file == null) throw new ArgumentNullException("file");
            if (input == null) throw new ArgumentNullException("input");

            _file = file;
            _input = input;

            SkipWhitespace();
        }

        #region Implementation of IDisposable

        public void Dispose()
        {
        }

        #endregion

        private void SkipWhitespace()
        {
            // While the character at the caret is a white space character.
            while (_caretPosition < _input.Length && char.IsWhiteSpace(_input[_caretPosition]))
                MoveCaret(1);

            if (_caretPosition >= _input.Length - 1)
                return;

            var firstTwoCharacter = _input.Substring(_caretPosition, 2);
            switch (firstTwoCharacter)
            {
                case "//":
                {
                    var endidx = _input.IndexOf("\n", _caretPosition, StringComparison.Ordinal);
                    MoveCaret(endidx < 0 ? _input.Length - _caretPosition : endidx - _caretPosition + 1);
                    SkipWhitespace();
                    break;
                }
                case "/*":
                {
                    var endidx = _input.IndexOf("*/", _caretPosition, StringComparison.Ordinal);
                    MoveCaret(endidx < 0 ? _input.Length - _caretPosition : endidx - _caretPosition + 2);
                    SkipWhitespace();
                    break;
                }
            }
        }

        private void MoveCaret(int count)
        {
            if (count < 0)
                throw new ArgumentOutOfRangeException("count", count, "value must be positive");


            for (var i = 0; i < count && _caretPosition < _input.Length; i++)
            {
                // If the character at the caret is a line break, increase the line number and reset the column number.
                // Otherwise increase the column number.
                if (_input[_caretPosition] == '\n')
                {
                    _line++;
                    _column = 1;
                }
                else
                    _column++;

                // Move the caret.
                _caretPosition ++;
            }
        }

        private void MoveToNextToken()
        {
            // If any tokens were pushed back to the tokenizer, move trough these first.
            if (_pushedTokens.Count > 0)
            {
                Current = _pushedTokens.Pop();
                return;
            }

            // Find the first matching token type.
            foreach (var tokenData in TokenTypes)
            {
                var match = tokenData.Pattern.Match(_input, _caretPosition);

                if (match.Success)
                {
                    Current = new Token(tokenData.Type, match.Groups[tokenData.ContentGroup].Value, _file, _line,
                        _column);

                    MoveCaret(match.Groups[0].Length);
                    SkipWhitespace();
                    return;
                }
            }

            // If no token type was found, the character at the caret is a token.
            var token = _input.Substring(_caretPosition, 1);
            Current = new Token(TokenType.Token, token, _file, _line, _column);
            MoveCaret(1);
            SkipWhitespace();
        }

        /// <summary>
        ///     Pushes the specified token.
        /// </summary>
        /// <param name="token">The token.</param>
        public void Push(Token token)
        {
            // Push the current token to the stack and set the current token to the pushed token.
            if (Current != null)
                _pushedTokens.Push(Current);

            Current = token;
        }

        #region Implementation of IEnumerator

        /// <summary>
        ///     Advances the enumerator to the next element of the collection.
        /// </summary>
        /// <returns>
        ///     true if the enumerator was successfully advanced to the next element; false if the enumerator has passed the end of
        ///     the collection.
        /// </returns>
        public bool MoveNext()
        {
            if (_caretPosition >= _input.Length && _pushedTokens.Count == 0)
            {
                Current = null;
                return false;
            }

            MoveToNextToken();
            return true;
        }

        /// <summary>
        ///     Sets the enumerator to its initial position, which is before the first element in the collection.
        /// </summary>
        public void Reset()
        {
            _column = 1;
            _line = 1;
            _caretPosition = 0;

            _pushedTokens.Clear();
            Current = null;

            SkipWhitespace();
        }

        /// <summary>
        ///     Gets the element in the collection at the current position of the enumerator.
        /// </summary>
        public Token Current { get; private set; }

        /// <summary>
        ///     Gets the element in the collection at the current position of the enumerator.
        /// </summary>
        object IEnumerator.Current
        {
            get { return Current; }
        }

        #endregion
    }
}