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

        public EarleStackFrame(EarleRuntime runtime, EarleFunction function, int callerIp, EarleStackFrame superFrame, EarleThread thread, EarleValue target)
        {
            if(runtime == null)
                throw new ArgumentNullException(nameof(runtime));
            if(thread == null)
                throw new ArgumentNullException(nameof(thread));
            Runtime = runtime;
            ParentFrame = superFrame;
            CallerIP = callerIp;
            Function = function;
            Thread = thread;
            Target = target;
        }

        public EarleRuntime Runtime { get; }

        public EarleValue Target { get; }

        public EarleStackFrame ParentFrame { get; }

        public int CallerIP { get; }

        public EarleStackFrameExecutor SubFrame { get; set; }

        public EarleThread Thread { get; }

        public EarleFunction Function { get; }

        public Stack<EarleRuntimeScope> Scopes { get; } = new Stack<EarleRuntimeScope>();

        public Stack<EarleValue> Stack { get; } = new Stack<EarleValue>();

        public int CIP { get; set; }

        internal EarleStackFrame SpawnSubFrame(EarleFunction function, int callerIp, EarleValue target)
        {
            return new EarleStackFrame(Runtime, function, callerIp, this, Thread, target);
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

