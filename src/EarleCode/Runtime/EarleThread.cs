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

namespace EarleCode.Runtime
{
    public class EarleThread
    {
        private static int _threads;

        public EarleThread(EarleCompletionHandler completionHandler)
        {
            ThreadId = _threads++;
            CompletionHandler = completionHandler;
        }

        public EarleThread(IEarleStackFrameExecutor executor, EarleCompletionHandler completionHandler)
            : this(completionHandler)
        {
            if (executor == null)
                throw new ArgumentNullException(nameof(executor));
            Executor = executor;
        }

        public int ThreadId { get; private set; }

        public IEarleStackFrameExecutor Executor { get; private set; }

        public EarleCompletionHandler CompletionHandler { get; }

        public bool IsAlive { get; private set; } = true;

        internal void AttachExecutor(IEarleStackFrameExecutor executor)
        {
            if (executor == null)
                throw new ArgumentNullException(nameof(executor));
            Executor = executor;
        }

        public EarleValue? Run()
        {
            var result = Executor.Run();

            if (result == null)
            {
                if (IsAlive)
                    Executor.Frame.Runtime.EnqueueThread(this);
            }
            else
            {
                CompletionHandler?.Invoke(result.Value);
            }

            return result;
        }

        public void Kill()
        {
            IsAlive = false;
        }
    }
}