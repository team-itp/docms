using System.Threading;
using System.Threading.Tasks;

namespace Docms.Client.Operations
{
    public interface IOperation
    {
        Task ExecuteAsync(CancellationToken cancellationToken = default);
    }
}
