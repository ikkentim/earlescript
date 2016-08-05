using System;
using System.Collections.Generic;
using EarleCode.Runtime.Values;

namespace EarleCode.Runtime
{
    public sealed class EarleStackFrame
    {
        public const int RootFrameIP = -4;
        public const int RootCallIP = -1;
        public const int ThreadFrameIP = -3;

        public EarleStackFrame(EarleRuntime runtime, EarleFunction function, IEarleStackFrameExecutor executor, int callerIp, EarleStackFrame superFrame, EarleThread thread)
        {
            if(runtime == null)
                throw new ArgumentNullException(nameof(runtime));
            if(thread == null)
                throw new ArgumentNullException(nameof(thread));
            Runtime = runtime;
            ParentFrame = superFrame;
            Executor = executor;
            CallerIP = callerIp;
            Function = function;
            Thread = thread;
        }

        public EarleRuntime Runtime { get; }

        public EarleThread Thread { get; }

        public EarleFunction Function { get; }

        public IEarleStackFrameExecutor Executor { get; }

        public EarleStackFrame ParentFrame { get; }

        public EarleStackFrame ChildFrame { get; set; }

        public int CallerIP { get; }


        internal EarleStackFrame SpawnChild(EarleFunction function, IEarleStackFrameExecutor executor, int callerIp)
        {
            return new EarleStackFrame(Runtime, function, executor, callerIp, this, Thread);
        }

        public EarleStackTrace GetStackTrace()
        {
            var frames = new List<EarleStackFrame>();

            var current = this;
            while(current != null)
            {
                frames.Add(current);
                current = current.ParentFrame;
            }

            return new EarleStackTrace(frames.ToArray());
        }
    }
}

