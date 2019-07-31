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

namespace EarleCode.Runtime.Instructions
{
    /// <summary>
    ///     Represents the THREAD.T instruction which spawns a new thread and call the function stored on the top of the stack
    ///     with the specified number of arguments stored below the top of the stack.
    /// </summary>
    /// <seealso cref="EarleCode.Runtime.Instructions.ThreadInstruction" />
    internal class ThreadWithoutTargetInstruction : ThreadInstruction
    {
        #region Overrides of ThreadInstruction

        /// <summary>
        ///     Gets a value indicating whether this call has target.
        /// </summary>
        protected override bool HasTarget => false;

        #endregion
    }
}