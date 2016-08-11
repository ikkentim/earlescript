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

using EarleCode.Compiler.Lexing;
using EarleCode.Runtime.Instructions;
using EarleCode.Runtime.Operators;

namespace EarleCode.Compiler.Parsers
{
	internal class AssignmentExpressionParser : ExpressionParser
	{
		protected virtual void YieldDuplicate()
		{
			Yield(OpCode.Duplicate);
		}

		#region Overrides of Parser

		protected override bool RequiresScope => true;

		protected override void Parse()
		{
			// Output:
			// ?

			OpCode modOperator = OpCode.Nop;
			var unaryModOperatorIsPrefix = true;

			if (SyntaxMatches("OPERATOR_MOD_ASSIGNMENT"))
			{
				modOperator = GetOperator(EarleOperatorType.AssignmentModOperator);
			}

			Lexer.AssertToken(TokenType.Identifier);

			var name = Lexer.Current.Value;
			Lexer.AssertMoveNext();

			CompiledBlock read, write;
			ParseDereference(out read, out write);

			if (modOperator == OpCode.Nop && SyntaxMatches("OPERATOR_MOD_ASSIGNMENT"))
			{
				modOperator = GetOperator(EarleOperatorType.AssignmentModOperator);
				unaryModOperatorIsPrefix = false;
			}

			if (modOperator != OpCode.Nop)
			{
				// Read
				PushRead(name, read);

				// Postfix duplicate
				if (!unaryModOperatorIsPrefix)
					YieldDuplicate();

				// Operator call
				Yield(OpCode.PushOne);
				Yield(modOperator);

				// Prefix dupelicate
				if (unaryModOperatorIsPrefix)
					YieldDuplicate();

				// Write
				PushWrite(name, write);
			}
			else
			{
				OpCode unaryOperator = OpCode.Nop;
				if (SyntaxMatches("OPERATOR_ASSIGNMENT"))
					unaryOperator = GetOperator(EarleOperatorType.AssignmentOperator);

				Lexer.SkipToken(TokenType.Token, "=");

				if (unaryOperator != OpCode.Nop)
				{
					PushRead(name, read);
				}

				Parse<ExpressionParser>();

				if (unaryOperator != OpCode.Nop)
					Yield(unaryOperator);

				YieldDuplicate();

				PushWrite(name, write);
			}
		}

		#endregion
	}
}