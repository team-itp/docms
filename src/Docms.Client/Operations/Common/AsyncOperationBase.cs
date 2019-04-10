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
                var task = Task.Run(() => ExecuteAsync(cts.Token));
                task.ContinueWith(t => tcs.TrySetResult(null), TaskContinuationOptions.OnlyOnRanToCompletion);
                task.ContinueWith(t => tcs.TrySetException(t.Exception.InnerException), TaskContinuationOptions.OnlyOnFaulted);
                task.ContinueWith(t => tcs.TrySetCanceled(), TaskContinuationOptions.OnlyOnCanceled);
                Task.WaitAny(tcs.Task, task);
                tcs.Task.Wait(10);
            }
            catch
            {
            }
            finally
            {
                if (!tcs.Task.IsCompleted)
                {
                    tcs.TrySetResult(null);
                }
                isFinished = true;
            }

        }

        protected abstract Task ExecuteAsync(CancellationToken token);
    }
}