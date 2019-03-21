using System;
using System.Threading;
using System.Threading.Tasks;

namespace Docms.Client.Operations
{
    public abstract class AsyncOperationBase : IOperation
    {
        private TaskCompletionSource<object> tcs;
        private CancellationTokenSource cts;
        private bool isStarted;
        private bool isFinished;

        public AsyncOperationBase(CancellationToken cancellationToken = default(CancellationToken))
        {
            IsAborted = cancellationToken.IsCancellationRequested;
            if (IsAborted)
            {
                Task = Task.FromCanceled(cancellationToken);
                isFinished = true;
            }
            else
            {
                tcs = new TaskCompletionSource<object>();
                cts = new CancellationTokenSource();
                Task = tcs.Task;
            }
            cancellationToken.Register(() => Abort());
        }

        public Task Task { get; private set; }
        public bool IsAborted { get; private set; }

        public void Abort()
        {
            if (!IsAborted && !isFinished)
            {
                cts.Cancel();
                if (!isStarted)
                {
                    // 処理が始まっていない場合のみタスクをキャンセル状態にする
                    IsAborted = true;
                    tcs.SetCanceled();
                }
                isFinished = true;
            }
        }

        public void Start()
        {
            if (IsAborted)
            {
                return;
            }

            if (isStarted)
            {
                throw new InvalidOperationException();
            }
            isStarted = true;

            try
            {
                var task = Task.Run(() => ExecuteAsync(cts.Token))
                    .ContinueWith(t => tcs.SetResult(null), TaskContinuationOptions.OnlyOnRanToCompletion)
                    .ContinueWith(t => tcs.SetCanceled(), TaskContinuationOptions.OnlyOnCanceled)
                    .ContinueWith(t => tcs.SetException(t.Exception.InnerException));
                task.Wait();
            }
            catch
            {
            }
            finally
            {
                isFinished = true;
            }

        }

        protected abstract Task ExecuteAsync(CancellationToken token);
    }
}