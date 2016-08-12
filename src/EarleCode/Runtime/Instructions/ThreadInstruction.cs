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
    ///     Represents the THREAD instruction which spawns a new thread and call the function stored on the top of the stack
    ///     with the target stored below the top of the stack and with the specified number of arguments stored below the
    ///     target.
    /// </summary>
    /// <seealso cref="CallInstruction" />
    internal class ThreadInstruction : CallInstruction
    {
        #region Overrides of CallInstruction

        /// <summary>
        ///     This method is invoked when the instruction needs to be run.
        /// </summary>
        protected override void Handle()
        {
            var ip = Frame.Executor.CIP - 1;
            var thread = new EarleThread(null);
            var rootFrame = new EarleStackFrame(Frame.Runtime, Frame.Function, null, ip, Frame, thread);
            var frame = CreateFrameExecutor(rootFrame, EarleStackFrame.ThreadFrameIP);

            if (frame == null)
                return;
            thread.AttachExecutor(frame);

            Frame.Runtime.EnqueueThread(thread);
        }

        #endregion
    }
}