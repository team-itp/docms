using System.Threading;
using System.Threading.Tasks;

namespace Docms.Client.Operations
{
    public abstract class AsyncOperationBase : OperationBase
    {
        public AsyncOperationBase(CancellationToken cancellationToken = default(CancellationToken)) : base(cancellationToken)
        {
        }

        protected override void Execute(CancellationToken cancellationToken)
        {
            ExecuteAsync(cancellationToken).GetAwaiter().GetResult();
        }

        protected abstract Task ExecuteAsync(CancellationToken token);
    }
}