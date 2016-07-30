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
    internal class ThreadInstruction : CallInstruction
    {
        protected override void Handle()
        {
            var thread = new EarleThread(null);
            var rootFrame = new EarleStackFrame(Frame.Frame.Runtime, thread, EarleValue.Undefined);
            var frame = CreateFrameExecutor(rootFrame);

            if(frame == null)
                return;
            thread.AttachFrame(frame);

            Frame.Frame.Runtime.StartThread(thread);
        }
    }
    
}