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
using System.Linq;

namespace EarleCode.Compiler.Lexing
{
	public class Token
	{
		public Token(TokenType type, string value, string file, int line, int column)
		{
			if (file == null) throw new ArgumentNullException(nameof(file));
			Type = type;
			Value = value;
			File = file;
			Line = line;
			Column = column;
		}

		public int Line { get; }

		public int Column { get; }

		public TokenType Type { get; }

		public string Value { get; }

		public string File { get; }

		public bool Is(TokenType type, params string[] values)
		{
			return Is(type) && (values == null || values.Contains(Value));
		}

		public bool Is(TokenType type, string value)
		{
			return Type == type && Value == value;
		}

		public bool Is(TokenType type)
		{
			return Type == type;
		}

		#region Overrides of Object

		public override string ToString()
		{
			return $"{Line}:{Column}: {Type} `{Value}`";
		}

		#endregion
	}
}