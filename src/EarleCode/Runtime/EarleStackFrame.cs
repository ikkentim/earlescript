using System;
using EarleCode.Runtime.Values;

namespace EarleCode.Runtime
{
    public class EarleStackFrame
    {
        public EarleStackFrame(EarleRuntime runtime, EarleThread thread, EarleValue target)
        {
            if(runtime == null)
                throw new ArgumentNullException(nameof(runtime));
            if(thread == null)
                throw new ArgumentNullException(nameof(thread));
            Runtime = runtime;
            Thread = thread;
            Target = target;
        }

        public EarleRuntime Runtime { get; }

        public EarleValue Target { get; }

        public EarleStackFrameExecutor SubFrame { get; set; }

        public EarleThread Thread { get; }

        internal EarleStackFrame SpawnSubFrame(EarleValue target)
        {
            return new EarleStackFrame(Runtime, Thread, target);
        }
    }
}

