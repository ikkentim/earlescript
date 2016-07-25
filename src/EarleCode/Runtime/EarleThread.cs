namespace EarleCode.Runtime
{
    public class EarleThread
    {
        public EarleThread(EarleStackFrameExecutor frame, EarleCompletionHandler completionHandler)
        {
            Frame = frame;
            CompletionHandler = completionHandler;
        }

        public EarleStackFrameExecutor Frame { get; }

        public EarleCompletionHandler CompletionHandler { get; }
    }
}

