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
	internal class StatementWaitParser : Parser, ISimpleStatement
	{
		#region Overrides of Parser

		protected override void Parse()
		{
			// Output:
			// EXPRESSION   (?)
			// PUSH_R       (?)
			// CALL N       (4)

			var lineNumber = Lexer.Current.Line;
			Lexer.SkipToken(TokenType.Identifier, "wait");
			Parse<ExpressionParser>();
			PushCallWithoutTarget(null, "wait", 1, lineNumber);
		}

		#endregion
	}
}