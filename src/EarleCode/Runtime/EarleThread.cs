using EarleCode.Runtime.Values;

namespace EarleCode.Runtime
{
    public class EarleThread
    {
        private static int _threads = 0;
        public EarleThread(EarleCompletionHandler completionHandler)
        {
            ThreadId = _threads++;
            CompletionHandler = completionHandler;
        }

        public EarleThread(IEarleStackFrameExecutor frame, EarleCompletionHandler completionHandler) : this(completionHandler)
        {
            Frame = frame;
        }

        public int ThreadId { get; private set; }

        public IEarleStackFrameExecutor Frame { get; private set; }

        public EarleCompletionHandler CompletionHandler { get; }

        public bool IsAlive { get; private set; } = true;

        internal void AttachFrame(IEarleStackFrameExecutor frame)
        {
            Frame = frame;
        }

        public EarleValue? Run()
        {
            var result = Frame.Run();

            if(result == null)
            {
                if(IsAlive)
                    Frame.Frame.Runtime.EnqueueThread(this);
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

