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

        public EarleThread(EarleStackFrameExecutor frame, EarleCompletionHandler completionHandler) : this(completionHandler)
        {
            Frame = frame;
        }

        public int ThreadId { get; private set; }
        public EarleStackFrameExecutor Frame { get; private set; }

        public EarleCompletionHandler CompletionHandler { get; }

        public bool IsAlive { get; private set; } = true;

        internal void AttachFrame(EarleStackFrameExecutor frame)
        {
            Frame = frame;
        }

        public void Kill()
        {
            IsAlive = false;
        }
    }
}

