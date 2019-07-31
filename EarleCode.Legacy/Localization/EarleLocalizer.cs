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
using EarleCode.OldCompiler.Lexing;

namespace EarleCode.Localization
{
	public class EarleLocalizer
	{
		private readonly Dictionary<Tuple<string, string>, string> _values =
			new Dictionary<Tuple<string, string>, string>();

		public string Key { get; set; }

		public void LoadFromFile(string fileName, string fileContents, string prefix)
		{
			var lexer = new Lexer(fileName, fileContents);

			string reference = null;
			while (lexer.MoveNext())
			{
				if (lexer.Current.Is(TokenType.Identifier, "REFERENCE"))
				{
					lexer.AssertMoveNext();
					lexer.AssertToken(TokenType.Identifier);
					reference = lexer.Current.Value;

					continue;
				}

				lexer.AssertToken(TokenType.Identifier);
				var key = lexer.Current.Value;
				lexer.AssertMoveNext();
				lexer.AssertToken(TokenType.StringLiteral);
				_values[new Tuple<string, string>(key, $"{prefix}{reference}")] = lexer.Current.Value;
			}
		}

		public string Localize(string reference)
		{
			if (Key == null || reference == null)
				return reference;

			string result;
			return _values.TryGetValue(new Tuple<string, string>(Key, reference), out result)
				? result
				: reference;
		}
	}
}