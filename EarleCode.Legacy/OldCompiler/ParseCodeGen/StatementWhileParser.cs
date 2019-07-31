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
using EarleCode.Runtime.Instructions;

namespace EarleCode.OldCompiler.ParseCodeGen
{
	internal class StatementWhileParser : Parser
	{
		#region Overrides of Parser

		protected override void Parse()
		{
			// Output:
			// EXPRESSION   (expressionLength)
			// JUMP_FALSE N (5)
			// CODE_BLOCK   (block.Length)
			// JUMP N       (5)

			Lexer.SkipToken(TokenType.Identifier, "while");
			Lexer.SkipToken(TokenType.Token, "(");
			var expressionLength = Parse<ExpressionParser>();
			Lexer.SkipToken(TokenType.Token, ")");
			var block = CompileBlock(EarleCompileOptions.Loop);
			Yield(OpCode.JumpIfFalse);
			Yield(block.Length + 5);
			Yield(block, true, 5, true, -block.Length - expressionLength - 5);
			Yield(OpCode.Jump);
			Yield(-block.Length - expressionLength - 10);
		}

		#endregion
	}
}