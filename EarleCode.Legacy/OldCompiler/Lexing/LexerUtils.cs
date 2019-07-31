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

using System.Diagnostics;
using System.Linq;
using EarleCode.OldCompiler.ParseCodeGen;

namespace EarleCode.OldCompiler.Lexing
{
	public static class LexerUtils
	{
		[DebuggerHidden]
		public static void AssertMoveNext(this ILexer lexer)
		{
			if (!lexer.MoveNext())
				throw new ParseException("Unexpected end of file");
		}

		[DebuggerHidden]
		public static void AssertToken(this ILexer lexer, TokenType type, string value)
		{
			if (lexer.Current.Type != type || lexer.Current.Value != value)
				throw new ParseException(lexer.Current, "Unexpected token");
		}

		[DebuggerHidden]
		public static void AssertToken(this ILexer lexer, TokenType type, params string[] values)
		{
			if (lexer.Current.Type != type || !values.Contains(lexer.Current.Value))
				throw new ParseException(lexer.Current, "Unexpected token");
		}

		[DebuggerHidden]
		public static void AssertToken(this ILexer lexer, TokenType type)
		{
			if (lexer.Current.Type != type)
				throw new ParseException(lexer.Current, "Unexpected token");
		}

		[DebuggerHidden]
		public static void SkipToken(this ILexer lexer, TokenType type, string value)
		{
			AssertToken(lexer, type, value);
			AssertMoveNext(lexer);
		}

		[DebuggerHidden]
		public static void SkipToken(this ILexer lexer, TokenType type, params string[] values)
		{
			AssertToken(lexer, type, values);
			AssertMoveNext(lexer);
		}

		[DebuggerHidden]
		public static void SkipToken(this ILexer lexer, TokenType type)
		{
			AssertToken(lexer, type);
			AssertMoveNext(lexer);
		}
	}
}