using System;
using EarleCode.Runtime.Values;

namespace EarleCode.Runtime
{
    public class EarleStackFrame
    {
        public EarleStackFrame(EarleRuntime runtime, EarleFile file, EarleThread thread, EarleValue target)
        {
            if(runtime == null)
                throw new ArgumentNullException(nameof(runtime));
            if(thread == null)
                throw new ArgumentNullException(nameof(thread));
            Runtime = runtime;
            File = file;
            Thread = thread;
            Target = target;
        }

        public EarleRuntime Runtime { get; }

        public EarleValue Target { get; }

        public EarleStackFrameExecutor SubFrame { get; set; }

        public EarleThread Thread { get; }

        public EarleFile File { get; }

        internal EarleStackFrame SpawnSubFrame(EarleFile file, EarleValue target)
        {
            return new EarleStackFrame(Runtime, file, Thread, target);
        }
    }
}

