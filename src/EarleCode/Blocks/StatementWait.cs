// EarleCode
// Copyright 2015 Tim Potze
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

namespace EarleCode.Blocks
{
    public class StatementWait : Block
    {
        private readonly float _time;

        public StatementWait(IScriptScope scriptScope, float time) : base(scriptScope)
        {
            _time = time;
        }

        #region Overrides of Block

        public override InvocationResult Invoke(Runtime runtime, IEarleContext context)
        {
            return _time <= 0
                ? InvocationResult.Empty
                : new InvocationResult(new IncompleteInvocationResult(context, null)
                {
                    Event = new WaitInvocationAwaitableEvent(_time)
                });
        }

        public override InvocationResult Continue(Runtime runtime, IncompleteInvocationResult incompleteInvocationResult)
        {
            return incompleteInvocationResult.Event.IsReady()
                ? InvocationResult.Empty
                : new InvocationResult(incompleteInvocationResult);
        }
        
        public override string ToString()
        {
            return $"wait {_time};";
        }

        #endregion

        private class WaitInvocationAwaitableEvent : IInvocationAwaitableEvent
        {
            public DateTime StartTime { get; }
            public float Time { get; }

            public WaitInvocationAwaitableEvent(float time)
            {
                StartTime = DateTime.Now;
                Time = time;
            }

            #region Implementation of IInvocationAwaitableEvent

            public bool IsReady()
            {
                return (DateTime.Now - StartTime).TotalSeconds > Time;
            }

            #endregion
        }
    }
}