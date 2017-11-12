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

using EarleCode.OldCompiler.Lexing;

namespace EarleCode.OldCompiler.ParseCodeGen
{
	internal class CallExpressionParser : Parser
	{
		protected bool LastCallWasThreaded { get; private set; }

		#region Overrides of Parser

		protected override void Parse()
		{
			// Output:
			// TARGET?              (?)
			// ARGUMENTS            (?) |
			// REFERENCE            (?) |
			// (CALL|THREAD)(_T) N  (5) |__ repeats

			LastCallWasThreaded = false;
			var hasTarget = false;

			if (!SyntaxMatches("FUNCTION_CALL_PART") && !Lexer.Current.Is(TokenType.Identifier, "thread"))
			{
				hasTarget = true;
				Parse<FunctionTargetExpressionParser>();
			}

			if (!SyntaxMatches("FUNCTION_CALL_PART") && Lexer.Current.Is(TokenType.Identifier, "thread"))
			{
				LastCallWasThreaded = true;
				Lexer.AssertMoveNext();
			}

			while (SyntaxMatches("FUNCTION_CALL_PART"))
			{
				var referenceBuffer = ParseToBuffer<FunctionReferenceExpressionParser>();

				var lineNumber = Lexer.Current.Line;
				Lexer.SkipToken(TokenType.Token, "(");

				var arguments = 0;
				while (!Lexer.Current.Is(TokenType.Token, ")"))
				{
					Parse<ExpressionParser>();
					arguments++;

					if (Lexer.Current.Is(TokenType.Token, ")"))
						break;

					Lexer.SkipToken(TokenType.Token, ",");
				}

				Lexer.SkipToken(TokenType.Token, ")");
				Yield(referenceBuffer);

				if (hasTarget)
					PushCall(arguments, lineNumber, LastCallWasThreaded);
				else
					PushCallWithoutTarget(arguments, lineNumber, LastCallWasThreaded);

				if (LastCallWasThreaded)
					break;

				if (!SyntaxMatches("FUNCTION_CALL_PART") && Lexer.Current.Is(TokenType.Identifier, "thread"))
				{
					LastCallWasThreaded = true;
					Lexer.AssertMoveNext();
					AssertSyntaxMatches("FUNCTION_CALL_PART");
				}
			}
		}

		#endregion
	}
}