using System;
using System.Threading.Tasks;

namespace Docms.Client.Operations
{
    public class ResourceOperationDispatcher<T> : IResourceOperationDispatcher<T>
    {
        private OperationDispatcher _dispatcher;
        private T _resource;

        public ResourceOperationDispatcher(T resource)
        {
            _dispatcher = new OperationDispatcher();
            _resource = resource;
        }

        public Task<TResult> Execute<TResult>(Func<T, Task<TResult>> func)
        {
            var tcs = new TaskCompletionSource<TResult>();
            _dispatcher.Invoke(new ResourceOperation(async token =>
            {
                try
                {
                    var result = await func.Invoke(_resource).ConfigureAwait(false);
                    tcs.SetResult(result);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            }));
            return tcs.Task;
        }

        public Task Execute(Func<T, Task> func)
        {
            var tcs = new TaskCompletionSource<object>();
            _dispatcher.Invoke(new ResourceOperation(async token =>
            {
                try
                {
                    await func.Invoke(_resource).ConfigureAwait(false);
                    tcs.SetResult(null);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            }));
            return tcs.Task;
        }

        public void Dispose()
        {
            _dispatcher.Dispose();
        }
    }
}
