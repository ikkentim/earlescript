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

using EarleCode.Runtime.Values;

namespace EarleCode.Runtime.Instructions
{
    /// <summary>
    ///     Represents the READ.F instruction which pushes the value from the specified field from the structure on the top of
    ///     the stack.
    /// </summary>
    /// <seealso cref="EarleCode.Runtime.Instructions.Instruction" />
    internal class ReadFieldInstuction : Instruction
    {
        #region Overrides of Instruction

        /// <summary>
        ///     This method is invoked when the instruction needs to be run.
        /// </summary>
        protected override void Handle()
        {
            var structure = Pop();
            var field = GetString();
            Push(structure.As<IEarleStructure>()?.GetField(field)
                 ?? EarleValue.Undefined);
        }

        #endregion
    }
}