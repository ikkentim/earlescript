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
using System.Linq;
using EarleCode.Compiler.Lexing;
using EarleCode.Runtime.Instructions;

namespace EarleCode.Compiler.Parsers
{
	internal class StatementSwitchParser : Parser
	{
		protected override void Parse()
		{
			Lexer.SkipToken(TokenType.Identifier, "switch");
			Lexer.SkipToken(TokenType.Token, "(");
			var expressionBuffer = ParseToBuffer<ExpressionParser>();
			Lexer.SkipToken(TokenType.Token, ")");

			Lexer.SkipToken(TokenType.Token, "{");

			CompiledBlock defaultBlock = null;
			var caseBlocks = new Dictionary<string, Tuple<CompiledBlock, CompiledBlock>>();
			while (!Lexer.Current.Is(TokenType.Token, "}"))
			{
				if (SyntaxMatches("LABEL_CASE"))
				{
					Lexer.SkipToken(TokenType.Identifier, "case");
					var caseStringValue = (Lexer.Current.Type == TokenType.NumberLiteral ? "N" : "S") + Lexer.Current.Value;

					if (caseBlocks.ContainsKey(caseStringValue))
					{
						ThrowParseException("every case value may only appear once in a switch block");
					}

					var caseValue = ParseToBuffer<ExpressionParser>();
					Lexer.SkipToken(TokenType.Token, ":");
					var caseBlock = CompileBlock(EarleCompileOptions.SwitchCase);

					caseBlocks[caseStringValue] = new Tuple<CompiledBlock, CompiledBlock>(caseValue, caseBlock);
				}
				else if (SyntaxMatches("LABEL_DEFAULT"))
				{
					if (defaultBlock != null)
					{
						ThrowParseException("only one default label can be specified in a switch block");
					}

					Lexer.SkipToken(TokenType.Identifier, "default");
					Lexer.SkipToken(TokenType.Token, ":");

					defaultBlock = CompileBlock(EarleCompileOptions.SwitchCase);
				}
				else
				{
					ThrowUnexpectedTokenWithExpected("label or `}`");
				}
			}

			Lexer.SkipToken(TokenType.Token, "}");


			// output:
			// EXPRESSION
			//
			// ((case ...:))
			// DUP
			// EXP
			// PUSH_R operator==
			// CALL_T 2
			// JUMP_F
			// BLOCK
			//
			// ((case ...:))
			// JUMP
			// DUP
			// EXP
			// PUSH_R operator==
			// CALL 2
			// JUMP_F
			// BLOCK
			//
			// ((default:))
			// BLOCK
			//
			// POP

			Yield(expressionBuffer);
			int length = (defaultBlock?.Length ?? 0) + caseBlocks.Values
				.Sum(t => 12 + t.Item1.Length + t.Item2.Length);
			var i = 0;
			var count = caseBlocks.Count;
			foreach (var caseBlock in caseBlocks.Values)
			{
				length -= 12 + caseBlock.Item1.Length + caseBlock.Item2.Length;

				if (i != 0)
					PushJump(caseBlock.Item1.Length + 7); // length: 5

				Yield(OpCode.Duplicate); // length: 1
				Yield(caseBlock.Item1); // length: ?

				Yield(OpCode.CheckEqual); // length: 1
				PushJump(false, caseBlock.Item2.Length + (i == count - 1 ? 0 : 5)); // length: [op JUMP_F]+ [n]4 == 5

				Yield(caseBlock.Item2, true, length); // length: ?

				i++;
			}

			if (defaultBlock != null)
				Yield(defaultBlock, true);

			Yield(OpCode.Pop);
		}
	}
}