﻿// EarleCode
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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace EarleCode.Compiler.Lexing
{
	public class Lexer : ILexer
	{
		private static readonly TokenTypeData[] _tokenTypes;
		private readonly string _file;
		private readonly string _input;
		private int _caretPosition;
		private int _column;
		private int _line;
		private Stack<Token> _pushedTokens = new Stack<Token>();

		static Lexer()
		{
			// Fill the token types array.
			_tokenTypes = new[]
			{
				new TokenTypeData(@"\G[a-zA-Z_][a-zA-Z0-9_]*", TokenType.Identifier),
				new TokenTypeData(@"\G([0-9]*\.?[0-9]+)", TokenType.NumberLiteral, 1),
				new TokenTypeData(@"\G([""])((?:\\\1|.)*?)\1", TokenType.StringLiteral, 2)
			};
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="Lexer" /> class.
		/// </summary>
		/// <param name="file">The file.</param>
		/// <param name="input">The input.</param>
		/// <exception cref="ArgumentNullException">Thrown if file or input is null.</exception>
		public Lexer(string file, string input)
		{
			if (file == null) throw new ArgumentNullException(nameof(file));
			if (input == null) throw new ArgumentNullException(nameof(input));

			_file = file;
			_input = input;

			Reset();
		}

		/// <summary>
		///     Pushes the specified token.
		/// </summary>
		/// <param name="token">The token.</param>
		public void Push(Token token)
		{
			if (token == null) throw new ArgumentNullException(nameof(token));

			// Push the current token to the stack and set the current token to the pushed token.
			if (Current != null)
				_pushedTokens.Push(Current);

			Current = token;
		}

		#region Implementation of ICloneable

		[DebuggerHidden]
		public object Clone()
		{
			return new Lexer(_file, _input)
			{
				_pushedTokens = new Stack<Token>(_pushedTokens),
				_caretPosition = _caretPosition,
				_column = _column,
				_line = _line,
				Current = Current
			};
		}

		#endregion

		#region Implementation of IDisposable

		/// <summary>
		///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
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

			if (_caretPosition <= _input.Length - 2)
			{
				if (_input[_caretPosition] == '/' && _input[_caretPosition + 1] == '/')
				{
					var endIndex = _input.IndexOf("\n", _caretPosition, StringComparison.Ordinal);
					MoveCaret(endIndex < 0 ? _input.Length - _caretPosition : endIndex - _caretPosition + 1);
					SkipWhitespace();
				}
				else if (_input[_caretPosition] == '/' && _input[_caretPosition + 1] == '*')
				{
					var endIndex = _input.IndexOf("*/", _caretPosition, StringComparison.Ordinal);
					MoveCaret(endIndex < 0 ? _input.Length - _caretPosition : endIndex - _caretPosition + 2);
					SkipWhitespace();
				}
			}
		}

		private void MoveCaret(int count)
		{
			if (count < 0)
				throw new ArgumentOutOfRangeException(nameof(count), count, "value must be positive");


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
				_caretPosition++;
			}
		}

		private void MoveToNextToken()
		{
			// If any tokens were pushed back to the lexer, move trough these first.
			if (_pushedTokens.Count > 0)
			{
				Current = _pushedTokens.Pop();
				return;
			}

			// Find the first matching token type.
			foreach (var tokenData in _tokenTypes)
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

		#region Overrides of Object

		/// <summary>
		///     Returns a string that represents the current object.
		/// </summary>
		/// <returns>
		///     A string that represents the current object.
		/// </returns>
		public override string ToString()
		{
			return $@"Lexer {{Current = ""{Current}""}}";
		}

		#endregion

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
		object IEnumerator.Current => Current;

		#endregion
	}
}