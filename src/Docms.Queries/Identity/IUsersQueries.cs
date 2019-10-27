using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Docms.Queries.Identity
{
    public interface IUsersQueries : IDisposable
    {
        Task<User> FindByIdAsync(string userId, CancellationToken cancellationToken);
        Task<User> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken);
        Task<IEnumerable<User>> GetUsersInRoleAsync(string roleName, CancellationToken cancelleationToken);
    }
}
