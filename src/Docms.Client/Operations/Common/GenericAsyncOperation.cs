using System;
using System.Threading;
using System.Threading.Tasks;

namespace Docms.Client.Operations
{
    public class GenericAsyncOperation : AsyncOperationBase
    {
        private Func<CancellationToken, Task> func;

        public GenericAsyncOperation(Func<CancellationToken, Task> func, CancellationToken cancellationToken = default(CancellationToken)) : base(cancellationToken)
        {
            this.func = func;
        }

        protected override Task ExecuteAsync(CancellationToken token)
        {
            return func.Invoke(token);
        }
    }
}