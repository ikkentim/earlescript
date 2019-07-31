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

using System.Text.RegularExpressions;

namespace EarleCode.OldCompiler.Lexing
{
	internal class TokenTypeData
	{
		private readonly string _pattern;

		public TokenTypeData(Regex pattern, TokenType type)
		{
			Pattern = pattern;
			Type = type;
		}

		public TokenTypeData(string pattern, TokenType type, int contentGroup = 0)
			: this(new Regex(pattern), type)
		{
			_pattern = pattern;
			ContentGroup = contentGroup;
		}

		public int ContentGroup { get; }

		public Regex Pattern { get; }

		public TokenType Type { get; }

		#region Overrides of Object

		public override string ToString()
		{
			return $"{Type} `{_pattern}`";
		}

		#endregion
	}
}