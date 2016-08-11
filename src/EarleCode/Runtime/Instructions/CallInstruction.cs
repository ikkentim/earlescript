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

using System.Linq;
using EarleCode.Runtime.Values;

namespace EarleCode.Runtime.Instructions
{
	internal class CallInstruction : Instruction
	{
		protected virtual bool HasTarget => true;

		#region Overrides of Instruction

		protected override void Handle()
		{
			Frame.ChildFrame = CreateFrameExecutor(Frame, Frame.Executor.CIP - 1)?.Frame;
		}

		#endregion

		protected virtual IEarleStackFrameExecutor CreateFrameExecutor(EarleStackFrame superFrame, int callerIp)
		{
			var argumentCount = GetInt32();
			var value = Pop();
			var hasOverloads = false;
			EarleFunction function;

			// Unbox down to a function or function collection
			//while(value is EarleVariableReference || value is EarleBoxedValueReference)
			//{
			//    if(value is EarleVariableReference)
			//        value = Frame.Executor.GetValue((EarleVariableReference)value).Value;
			//    else if(value is EarleBoxedValueReference)
			//        value = ((EarleBoxedValueReference)value).GetField().Value;
			//}

			if (value.Is<EarleFunctionCollection>())
			{
				var functions = value.As<EarleFunctionCollection>();
				hasOverloads = functions.Count > 0;
				function = functions.GetBestOverload(argumentCount);
			}
			else
			{
				function = value.As<EarleFunction>();
			}

			if (function == null)
			{
				if (hasOverloads)
					Frame.Runtime.HandleWarning(
						$"No suitable overload cana be found of `{value.As<EarleFunctionCollection>().FirstOrDefault()?.Name}`.");
				else if (!value.HasValue)
					Frame.Runtime.HandleWarning("A null pointer cannot be invoked.");
				else
					Frame.Runtime.HandleWarning($"{value.Value?.GetType()} cannot be invoked.");

				for (var i = 0; i < argumentCount; i++)
					Pop();

				Push(EarleValue.Undefined);
				return null;
			}

			var args = new EarleValue[argumentCount];
			for (var i = 0; i < argumentCount; i++)
				args[argumentCount - 1 - i] = Pop();

			var target = HasTarget ? Pop() : EarleValue.Undefined;

			return function.CreateFrameExecutor(superFrame, callerIp, target, args);
		}
	}
}