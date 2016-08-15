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

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using EarleCode.Compiler.Lexing;
using EarleCode.Runtime.Instructions;
using EarleCode.Runtime.Operators;

namespace EarleCode.Compiler.Parsers
{
	internal class ExpressionParser : Parser
	{
		protected OpCode GetOperator(EarleOperatorType type)
		{
			var str = "";
			do
			{
				str += Lexer.Current.Value;
				Lexer.AssertMoveNext();
			} while (Lexer.Current.Is(TokenType.Token) && EarleOperators.GetMaxOperatorLength(type, str) > str.Length);

			return EarleOperators.GetOperator(type, str);
		}

		#region Overrides of Parser

		protected override void Parse()
		{
			// Output:
			// ?

			var operators = new Stack<OpCode>();

			for (;;)
			{
				ParseValue();


				if (SyntaxMatches("OPERATOR_AND"))
				{
					Lexer.SkipToken(TokenType.Token, "&");
					Lexer.SkipToken(TokenType.Token, "&");

					var block = ParseToBuffer<ExpressionParser>();
					PushJump(false, block.Length + 5);
					Yield(block);
					PushJump(5);
					PushInteger(0);

					return;
				}

				if (SyntaxMatches("OPERATOR_OR"))
				{
					Lexer.SkipToken(TokenType.Token, "|");
					Lexer.SkipToken(TokenType.Token, "|");

					var block = ParseToBuffer<ExpressionParser>();
					PushJump(true, block.Length + 5);
					Yield(block);
					PushJump(5);
					PushInteger(0);

					return;
				}

				if (SyntaxMatches("OPERATOR_BINARY"))
				{
					Lexer.AssertToken(TokenType.Token);

					var op = GetOperator(EarleOperatorType.BinaryOperator);

					if (op == OpCode.Nop)
						ThrowUnexpectedToken("-OPERATOR-");

					if (operators.Any() && EarleOperators.GetPriority(operators.Peek()) >= EarleOperators.GetPriority(op))
						do Yield(operators.Pop()); while (operators.Any());

					operators.Push(op);
				}
				else
				{
					while (operators.Any())
						Yield(operators.Pop());

					return;
				}
			}
		}

		#endregion

		#region Parse

		protected virtual bool ParseFunctionCall()
		{
			Parse<CallExpressionParser>();

			CompiledBlock read, write;
			ParseDereference(out read, out write);
			Yield(read);

			return true;
		}

		protected virtual void ParseDereference(out CompiledBlock readBlock, out CompiledBlock writeBlock)
		{
			var read = new List<byte>();
			var write = new List<byte>();

			var callLinesRead = new Dictionary<int, int>();
			var callLinesWrite = new Dictionary<int, int>();
			var referencedFiles = new List<string>();
			var requiresScope = false;

			for (;;)
			{
				if (Lexer.Current.Is(TokenType.Token, "."))
				{
					Lexer.AssertMoveNext();
					Lexer.AssertToken(TokenType.Identifier);
					var field = Lexer.Current.Value;
					Lexer.AssertMoveNext();

					var isLast = !Lexer.Current.Is(TokenType.Token, ".") && !Lexer.Current.Is(TokenType.Token, "[");

					read.Add((byte) OpCode.ReadField);
					read.AddRange(Encoding.ASCII.GetBytes(field));
					read.Add(0);

					write.Add(isLast ? (byte) OpCode.WriteField : (byte) OpCode.ReadField);
					write.AddRange(Encoding.ASCII.GetBytes(field));
					write.Add(0);
				}
				else if (Lexer.Current.Is(TokenType.Token, "["))
				{
					Lexer.AssertMoveNext();
					var buffer = ParseToBuffer<ExpressionParser>();
					Lexer.SkipToken(TokenType.Token, "]");

					foreach (var kv in buffer.CallLines)
					{
						callLinesRead[kv.Key + read.Count] = kv.Value;
						callLinesWrite[kv.Key + write.Count] = kv.Value;
					}

					referencedFiles.AddRange(buffer.ReferencedFiles.Where(f => !referencedFiles.Contains(f)));
					requiresScope |= buffer.RequiresScope;

					var isLast = !Lexer.Current.Is(TokenType.Token, ".") && !Lexer.Current.Is(TokenType.Token, "[");

					read.AddRange(buffer.PCode);
					read.Add((byte) OpCode.ReadIndex);

					write.AddRange(buffer.PCode);
					write.Add(isLast ? (byte) OpCode.WriteIndex : (byte) OpCode.ReadIndex);
				}
				else
				{
					break;
				}
			}

			readBlock = new CompiledBlock(read.ToArray(), callLinesRead, referencedFiles.ToArray(), new int[0], new int[0],
				requiresScope);
			writeBlock = new CompiledBlock(write.ToArray(), callLinesWrite, referencedFiles.ToArray(), new int[0], new int[0],
				requiresScope);
		}

		protected virtual void ParseValue()
		{
			OpCode unaryOperator = OpCode.Nop;

			// Sub-parsers (unary operators are not applicable)
			if (SyntaxMatches("ASSIGNMENT"))
			{
				Parse<AssignmentExpressionParser>();
				return;
			}


			// Unary operator parsing
			if (SyntaxMatches("OPERATOR_UNARY"))
			{
				Lexer.AssertToken(TokenType.Token);
				unaryOperator = EarleOperators.GetOperator(EarleOperatorType.UnaryOperator, Lexer.Current.Value);
				Lexer.AssertMoveNext();
			}

			if (SyntaxMatches("FUNCTION_CALL") && ParseFunctionCall())
			{
				// If parse is not allowed, move on to next else-if case
			}
			else if (SyntaxMatches("EXPLICIT_FUNCTION_IDENTIFIER"))
			{
				Parse<FunctionReferenceExpressionParser>();
				return;
			}
			else if (SyntaxMatches("KEYWORD"))
				ParseKeyword();
			else if (SyntaxMatches("VECTOR"))
			{
				var vectorSize = 3;
				var lineNumber = Lexer.Current.Line;
				Lexer.SkipToken(TokenType.Token, "(");
				Parse<ExpressionParser>();
				Lexer.SkipToken(TokenType.Token, ",");
				Parse<ExpressionParser>();
				if (Lexer.Current.Is(TokenType.Token, ")"))
				{
					vectorSize = 2;
				}
				else
				{
					Lexer.SkipToken(TokenType.Token, ",");
					Parse<ExpressionParser>();
				}

				PushCallWithoutTarget(null, $"createvector{vectorSize}", vectorSize, lineNumber);

				Lexer.SkipToken(TokenType.Token, ")");
			}
			else if (Lexer.Current.Is(TokenType.Token, "("))
			{
				Lexer.AssertMoveNext();
				Parse<ExpressionParser>();
				Lexer.SkipToken(TokenType.Token, ")");

				CompiledBlock read, write;
				ParseDereference(out read, out write);
				Yield(read);
			}
			else if (Lexer.Current.Is(TokenType.NumberLiteral) || Lexer.Current.Is(TokenType.StringLiteral))
			{
				ParseLiteral();
			}
			else if (Lexer.Current.Is(TokenType.Identifier))
			{
				var name = Lexer.Current.Value;
				Lexer.AssertMoveNext();

				CompiledBlock read, write;
				ParseDereference(out read, out write);
				PushRead(name, read);
			}
			else
				ThrowUnexpectedToken("-VALUE-");

			if (unaryOperator != OpCode.Nop)
				Yield(unaryOperator);
		}

		protected virtual void ParseKeyword()
		{
			switch (Lexer.Current.Value)
			{
				case "true":
					PushInteger(1);
					break;
				case "false":
					PushInteger(0);
					break;
				case "undefined":
					Yield(OpCode.PushUndefined);
					break;
				case "[":
					Lexer.AssertMoveNext();
					Lexer.AssertToken(TokenType.Token, "]");
					Yield(OpCode.PushArray);
					break;
				default:
					ThrowUnexpectedToken("-KEYWORD-");
					break;
			}
			Lexer.AssertMoveNext();
		}

		protected virtual void ParseLiteral()
		{
			switch (Lexer.Current.Type)
			{
				case TokenType.NumberLiteral:
					float fValue;
					int iValue;

					if (int.TryParse(Lexer.Current.Value, out iValue))
						PushInteger(iValue);
					else if (float.TryParse(Lexer.Current.Value, NumberStyles.AllowDecimalPoint,
						CultureInfo.InvariantCulture, out fValue))
						PushFloat(fValue);
					else
						ThrowUnexpectedToken(TokenType.NumberLiteral);
					break;
				case TokenType.StringLiteral:
					PushString(Lexer.Current.Value);
					break;
				default:
					ThrowUnexpectedToken(TokenType.NumberLiteral, TokenType.StringLiteral);
					break;
			}

			Lexer.AssertMoveNext();
		}

		#endregion
	}
}