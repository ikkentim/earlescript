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

        public EarleThread(IEarleStackFrameExecutor executor, EarleCompletionHandler completionHandler) : this(completionHandler)
        {
            if(executor == null)
                throw new System.ArgumentNullException(nameof(executor));
            Executor = executor;
        }

        public int ThreadId { get; private set; }

        public IEarleStackFrameExecutor Executor { get; private set; }

        public EarleCompletionHandler CompletionHandler { get; }

        public bool IsAlive { get; private set; } = true;

        internal void AttachExecutor(IEarleStackFrameExecutor executor)
        {
            if(executor == null)
                throw new System.ArgumentNullException(nameof(executor));
            Executor = executor;
        }

        public EarleValue? Run()
        {
            var result = Executor.Run();

            if(result == null)
            {
                if(IsAlive)
                    Executor.Frame.Runtime.EnqueueThread(this);
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

