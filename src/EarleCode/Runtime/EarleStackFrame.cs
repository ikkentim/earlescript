using System;
using EarleCode.Runtime.Values;

namespace EarleCode.Runtime
{
    public class EarleStackFrame
    {
        public EarleStackFrame(EarleRuntime runtime, EarleValue target)
        {
            Runtime = runtime;
            Target = target;
        }

        public EarleRuntime Runtime { get; }

        public EarleValue Target { get; }

        public EarleStackFrameExecutor SubFrame { get; set; }
    }
}

