using System;
using System.Threading;
using System.Threading.Tasks;

namespace Docms.Domain.SeedWork
{
    public interface IUnitOfWork : IDisposable
    {        
        Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default);
    }
}
