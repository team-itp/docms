using System;
using System.Threading;
using System.Threading.Tasks;

namespace Docms.Client.Operations
{
    public abstract class OperationBase : IOperation
    {
        private TaskCompletionSource<object> tcs;
        private CancellationTokenSource cts;
        private bool isStarted;
        private bool isFinished;

        public OperationBase(CancellationToken cancellationToken = default(CancellationToken))
        {
            IsAborted = cancellationToken.IsCancellationRequested;
            Progress = new Progress<int>();
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
            cancellationToken.Register(() => AbortInternal(false));
        }

        public Task Task { get; }
        public bool IsAborted { get; private set; }
        public Progress<int> Progress { get; }

        public void Abort()
        {
            AbortInternal(true);
        }

        private void AbortInternal(bool wait)
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
                if (wait)
                {
                    try
                    {
                        Task.Wait();
                    }
                    catch
                    {
                    }
                }
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
            ReportProgress(0);

            try
            {
                Execute(cts.Token);
                tcs.SetResult(null);
            }
            catch (AggregateException ex)
            {
                var innerException = ex.Flatten().InnerException;
                if (innerException is OperationCanceledException)
                {
                    tcs.SetCanceled();
                }
                else
                {
                    tcs.SetException(innerException);
                }
            }
            catch (OperationCanceledException)
            {
                tcs.SetCanceled();
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
            finally
            {
                isFinished = true;
                ReportProgress(100);
            }
        }

        protected void ReportProgress(int progress)
        {
            (Progress as IProgress<int>).Report(progress);
        }

        protected abstract void Execute(CancellationToken cancellationToken);
    }
}