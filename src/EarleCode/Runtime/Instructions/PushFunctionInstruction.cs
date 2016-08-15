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
using EarleCode.Runtime.Values;

namespace EarleCode.Runtime.Instructions
{
    /// <summary>
    ///     Represents the PUSH.C instruction which pushes the specified function to the stack.
    /// </summary>
    /// <seealso cref="EarleCode.Runtime.Instructions.Instruction" />
    internal class PushFunctionInstruction : Instruction
    {
        #region Overrides of Instruction

        /// <summary>
        ///     This method is invoked when the instruction needs to be run.
        /// </summary>
        protected override void Handle()
        {
            var refString = GetString();

            string file = null,
                name;

            // Split the refString formatted `file::name`.
            var splitIndex = refString.IndexOf("::", StringComparison.Ordinal);
            if (splitIndex >= 0)
            {
                file = refString.Substring(0, splitIndex);
                name = refString.Substring(splitIndex + 2);
            }
            else
                name = refString;

            if (file != null && file.Length == 0)
                file = null;

            Push((EarleValue) Frame.Executor.GetFunctionReference(file, name));
        }

        #endregion
    }
}