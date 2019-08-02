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
using System.Linq;
using System.Text.RegularExpressions;

namespace EarleCode.Compiling.Lexing
{
	/// <summary>
	///     Represents a simple regex-based lexer with keyword and multi-char symbols support.
	/// </summary>
	/// <remarks>Only 2-char multi-char symbols are currently supported.</remarks>
	public class Lexer : ILexer
	{
		private readonly string[] _keywords;
		private readonly string[] _multiCharSymbols;
		private readonly int _maxMultiCharSymbolLength;
		
		private readonly List<TokenTypeData> _tokenTypes = new List<TokenTypeData>();

		/// <summary>
		///     Initializes a new instance of the <see cref="Lexer" /> class.
		/// </summary>
		/// <param name="multiCharSymbols">Symbols which consist of multiple characters.</param>
		/// <param name="keywords">Keywords to be detected.</param>
		public Lexer(string[] multiCharSymbols, string[] keywords)
		{
			if(multiCharSymbols == null)
				multiCharSymbols = new string[0];
			
			if(keywords == null)
				keywords= new string[0];
			
			_multiCharSymbols = multiCharSymbols;
			_maxMultiCharSymbolLength = _multiCharSymbols.Length == 0 ? 0 : _multiCharSymbols.Max(s => s.Length);
				
			_keywords = keywords;

			SetRegex(new Dictionary<TokenType, Regex>
			{
				[TokenType.Identifier] = new Regex(@"\G[a-zA-Z_][a-zA-Z0-9_]*"),
				[TokenType.NumberLiteral] = new Regex(@"\G([0-9]*\.?[0-9]+)"),
				[TokenType.StringLiteral] = new Regex(@"\G([""])((?:\\\1|.)*?)\1"),
			});
		}

		/// <summary>
		///     Sets the regular expressions used to detect tokens.
		/// </summary>
		/// <param name="regexes">A set of regular expressions.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="regexes"/> is null.</exception>
		public void SetRegex(IDictionary<TokenType, Regex> regexes)
		{
			if (regexes == null) throw new ArgumentNullException(nameof(regexes));

			_tokenTypes.Clear();

			foreach (var reg in regexes)
			{
				_tokenTypes.Add(new TokenTypeData { Pattern = reg.Value, Type = reg.Key });
			}
		}
		
		#region Implementation of ILexer

		/// <summary>
		///     Tokenizes the specified <paramref name="input" /> string.
		/// </summary>
		/// <param name="input">The string to tokenize.</param>
		/// <param name="file">The source file to assign to the tokens in the result.</param>
		/// <returns>A collections of tokens.</returns>
		public IEnumerable<Token> Tokenize(string input, string file)
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

					if (tokenData.Type == TokenType.Identifier)
					{
						// Check for keywords in the keywords list.
						if (_keywords != null)
						{
							var anyMatch = false;
							
							foreach (var kw in _keywords)
							{
								if (kw == match.Groups[0].Value)
								{
									yield return new Token(TokenType.Keyword, kw, file, position.Line, position.Column);
									MoveCaret(kw.Length, input, ref position);
									SkipWhitespace(input, ref position);
									anyMatch = true;
									break;
								}
							}

							if (anyMatch)
								break;
						}
					}

					yield return new Token(tokenData.Type, match.Groups[0].Value, file, position.Line,
						position.Column);

					MoveCaret(match.Groups[0].Length, input, ref position);
					SkipWhitespace(input, ref position);
					break;
				}

				if (match?.Success ?? false)
					continue;

				// If no token type was found, a symbol must follow.
				string token = null;
				for (var l = _maxMultiCharSymbolLength; l > 1; l--)
				{
					if (input.Length < position.Caret + l)
						continue;

					var symbol = input.Substring(position.Caret, l);

					if (!_multiCharSymbols.Contains(symbol))
						continue;

					token = symbol;
					break;
				}

				if (token == null)
					token = input.Substring(position.Caret, 1);

				yield return new Token(default(TokenType), token, file, position.Line, position.Column);

				MoveCaret(token.Length, input, ref position);
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
			public TokenType Type;
		}

		private struct LexerPosition
		{
			public int Caret;
			public int Line;
			public int Column;
		}
	}
}