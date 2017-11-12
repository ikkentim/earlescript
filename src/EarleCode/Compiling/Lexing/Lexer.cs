// EarleCode
// Copyright 2017 Tim Potze
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
using System.Text.RegularExpressions;
using EarleCode.Utilities;

namespace EarleCode.Compiling.Lexing
{
    /// <summary>
    ///     Represents a regex-based lexer for tokens of type <typeparamref name="TTokenType" />. The tokens will be matched
    ///     against regular expressions attached to the token types using the <see cref="TokenRegexAttribute" />. If tokens
    ///     cannot be matched against any regular expression, the default token type will be used.
    /// </summary>
    /// <typeparam name="TTokenType">The type of the tokens resulting from the lexer.</typeparam>
    public class Lexer<TTokenType> : ILexer<TTokenType> where TTokenType : struct, IConvertible
    {
        private readonly List<TokenTypeData> _tokenTypes;

        /// <summary>
        ///     Initializes a new instance of the <see cref="Lexer{TTokenType}" /> class.
        /// </summary>
        public Lexer()
        {
            _tokenTypes = new List<TokenTypeData>();
            foreach (var type in typeof(TTokenType).GetEnumValues())
            {
                var attr = ((Enum) type).GetCustomAttribute<TokenRegexAttribute>();
                if (attr != null)
                    _tokenTypes.Add(new TokenTypeData
                    {
                        Pattern = new Regex(attr.Expression),
                        Type = (TTokenType) type
                    });
            }
        }

        #region Implementation of ILexer<TTokenType>

        /// <summary>
        ///     Tokenizes the specified <paramref name="input" /> string.
        /// </summary>
        /// <param name="input">The string to tokenize.</param>
        /// <param name="file">The source file to assign to the tokens in the result.</param>
        /// <returns>A collections of tokens.</returns>
        public IEnumerable<Token<TTokenType>> Tokenize(string input, string file)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));

            var position = new LexerPosition
            {
                Caret = 0,
                Line = 1,
                Column = 1
            };

            SkipWhitespace(input, ref position);

            while (position.Caret < input.Length)
            {
                Match match = null;
                // Find the first matching token type.
                foreach (var tokenData in _tokenTypes)
                {
                    match = tokenData.Pattern.Match(input, position.Caret);

                    if (!match.Success)
                        continue;

                    yield return new Token<TTokenType>(tokenData.Type, match.Groups[0].Value, file, position.Line,
                        position.Column);

                    MoveCaret(match.Groups[0].Length, input, ref position);
                    SkipWhitespace(input, ref position);
                    break;
                }

                if (match?.Success ?? false)
                    continue;

                // If no token type was found, the character at the caret is of the default token type.
                var token = input.Substring(position.Caret, 1);
                yield return new Token<TTokenType>(default(TTokenType), token, file, position.Line, position.Column);
                MoveCaret(1, input, ref position);
                SkipWhitespace(input, ref position);
            }
        }

        #endregion

        /// <summary>
        ///     Moves the carret past the current whitespace.
        /// </summary>
        /// <param name="input">The string to move the carret in.</param>
        /// <param name="carret">A reference to the position of the carret.</param>
        private void SkipWhitespace(string input, ref LexerPosition carret)
        {
            // While the character at the caret is a white space character.
            while (carret.Caret < input.Length && char.IsWhiteSpace(input[carret.Caret]))
                MoveCaret(1, input, ref carret);

            if (carret.Caret >= input.Length - 1)
                return;

            // Remove single- and multi-line comments.
            if (carret.Caret <= input.Length - 2)
                if (input[carret.Caret] == '/' && input[carret.Caret + 1] == '/')
                {
                    var endIndex = input.IndexOf("\n", carret.Caret, StringComparison.Ordinal);
                    MoveCaret(endIndex < 0 ? input.Length - carret.Caret : endIndex - carret.Caret + 1, input,
                        ref carret);
                    SkipWhitespace(input, ref carret);
                }
                else if (input[carret.Caret] == '/' && input[carret.Caret + 1] == '*')
                {
                    var endIndex = input.IndexOf("*/", carret.Caret, StringComparison.Ordinal);
                    MoveCaret(endIndex < 0 ? input.Length - carret.Caret : endIndex - carret.Caret + 2, input,
                        ref carret);
                    SkipWhitespace(input, ref carret);
                }
        }

        /// <summary>
        ///     Moves the carret the specified <paramref name="amount" />.
        /// </summary>
        /// <param name="amount">The number of characters to move.</param>
        /// <param name="input">The string to move the carret in.</param>
        /// <param name="carret">A reference to the position of the carret.</param>
        private void MoveCaret(int amount, string input, ref LexerPosition carret)
        {
            if (amount <= 0)
                return;

            for (var i = 0; i < amount && carret.Caret < input.Length; i++)
            {
                // If the character at the caret is a line break, increase the line number and reset the column number.
                // Otherwise increase the column number.
                if (input[carret.Caret] == '\n')
                {
                    carret.Line++;
                    carret.Column = 1;
                }
                else
                {
                    carret.Column++;
                }

                // Move the caret.
                carret.Caret++;
            }
        }

        private struct TokenTypeData
        {
            public Regex Pattern;
            public TTokenType Type;
        }

        private struct LexerPosition
        {
            public int Caret;
            public int Line;
            public int Column;
        }
    }
}