using Docms.Domain.SeedWork;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Docms.Application.Tests.Utils
{
    class NoUnitOfWork : IUnitOfWork
    {
        public void Dispose()
        {
        }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(1);
        }

        public Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(true);
        }
    }

    class MockRepository<T> : IRepository<T>
        where T : Entity, IAggregateRoot
    {
        private int _lastPublishedId;

        public MockRepository()
        {
            UnitOfWork = new NoUnitOfWork();
        }

        public IUnitOfWork UnitOfWork { get; }

        public List<T> Entities { get; } = new List<T>();

        public Task AddAsync(T entity)
        {
            var fi = typeof(Entity).GetField("_Id", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            fi.SetValue(entity, ++_lastPublishedId);
            Entities.Add(entity);
            return Task.FromResult(entity);
        }

        public Task<T> GetAsync(int id)
        {
            return Task.FromResult(Entities.FirstOrDefault(e => e.Id == id));
        }

        public Task UpdateAsync(T entity)
        {
            return Task.FromResult(entity);
        }
    }
}
