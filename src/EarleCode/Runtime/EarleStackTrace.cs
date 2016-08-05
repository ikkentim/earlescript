using System;
using System.Collections.Generic;
using EarleCode.Runtime.Values;

namespace EarleCode.Runtime
{
    public class EarleStackTrace
    {
        public EarleStackTrace(EarleStackFrame[] frames)
        {
            Frames = frames;
        }

        public EarleStackFrame[] Frames { get; }

        public override string ToString()
        {
            var result = string.Empty;

            foreach(var frame in Frames)
            {
                var callerIP = frame.CallerIP;
                switch(callerIP)
                {
                    case EarleStackFrame.RootCallIP:
                        result += " at (ROOT CALL)";
                        break;
                    case EarleStackFrame.ThreadFrameIP:
                        result += " at (THREAD START)";
                        break;
                    case EarleStackFrame.RootFrameIP:
                        break;
                    default:
                        {
                            var callerFileName = frame.ParentFrame?.Function?.File?.Name ?? "???";
                            var callerFunctionName = (frame.ParentFrame?.Function?.File?.Name ?? "???") + ":" + (frame.ParentFrame?.Function?.Name ?? "???");
                            var callerFunctionArgs = string.Join(",", frame.ParentFrame?.Function?.Parameters ?? new string[0]);

                            var line = 0;
                            frame.ParentFrame?.Function?.CallLines?.TryGetValue(callerIP, out line);

                            result += $" at {callerFunctionName}({callerFunctionArgs}) [0x{callerIP:X5}] in {callerFileName}:{line}";

                            break;
                        }
                }

                result += Environment.NewLine;
            }

            return result;
        }
    }
    
}
