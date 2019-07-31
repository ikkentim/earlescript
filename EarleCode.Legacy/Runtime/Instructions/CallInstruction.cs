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
    /// <summary>
    ///     Represents the CALL instruction which calls the function stored on the top of the stack with the target stored
    ///     below the top of the stack and with the specified number of arguments stored below the target.
    /// </summary>
    /// <seealso cref="Instruction" />
    internal class CallInstruction : Instruction
    {
        /// <summary>
        ///     Gets a value indicating whether this call has target.
        /// </summary>
        protected virtual bool HasTarget => true;

        #region Overrides of Instruction

        /// <summary>
        ///     This method is invoked when the instruction needs to be run.
        /// </summary>
        protected override void Handle()
        {
            Frame.ChildFrame = CreateFrameExecutor(Frame, Frame.Executor.CIP - 1)?.Frame;
        }

        #endregion

        /// <summary>
        ///     Creates the frame executor this instruction will invoke.
        /// </summary>
        /// <param name="superFrame">The super frame.</param>
        /// <param name="callerIp">The caller ip.</param>
        /// <returns>The newly created stack frame executor.</returns>
        protected virtual IEarleStackFrameExecutor CreateFrameExecutor(EarleStackFrame superFrame, int callerIp)
        {
            var argumentCount = GetInt32();

            var funcValue = Pop();

            // Pop the arguments of the stack.
            var args = new EarleValue[argumentCount];
            for (var i = 0; i < argumentCount; i++)
                args[argumentCount - 1 - i] = Pop();

            // Pop the call target of the stack.
            var target = HasTarget ? Pop() : EarleValue.Undefined;

            // Find the function to call.
            var function = funcValue.As<EarleFunctionCollection>()?.GetBestOverload(argumentCount)
                           ?? funcValue.As<EarleFunction>();

            // If no function was specified, show an error and push NULL onto the stack instead of the result.
            if (function == null)
            {
                // Throw a tailored warning.
                if (funcValue.As<EarleFunctionCollection>()?.Any() ?? false)
                    Frame.Runtime.HandleWarning(
                        $"No suitable overload can be found of `{funcValue.As<EarleFunctionCollection>().FirstOrDefault()?.Name}`.");
                else if (funcValue.As<EarleFunctionCollection>()?.Any() ?? false)
                    Frame.Runtime.HandleWarning("No suitable overload can be found.");
                else if (!funcValue.HasValue)
                    Frame.Runtime.HandleWarning("A null pointer cannot be invoked.");
                else
                    Frame.Runtime.HandleWarning($"{funcValue.Value?.GetType()} cannot be invoked.");

                // Push NULL onto the stack to as the result.
                Push(EarleValue.Undefined);
                return null;
            }

            // Create the frame executor of the function.
            return function.CreateFrameExecutor(superFrame, callerIp, target, args);
        }
    }
}