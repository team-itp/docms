using Docms.Queries.Identity;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Docms.Application.Tests.Utils
{
    class MockUsersQueries : IUsersQueries
    {
        private List<User> users = new List<User>();

        public void Create(User user)
        {
            users.Add(user);
        }

        public void Delete(User user)
        {
            users.Remove(user);
        }

        public void Dispose()
        {
        }

        public Task<User> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            return Task.FromResult(users.FirstOrDefault(e => e.Id == userId));
        }

        public Task<User> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            return Task.FromResult(users.FirstOrDefault(e => e.AccountName.ToUpperInvariant() == normalizedUserName));
        }

        public Task<IEnumerable<User>> GetUsersInRoleAsync(string roleName, CancellationToken cancelleationToken)
        {
            return Task.FromResult(users.Where(u => u.Roles.Any(r => r.ToUpperInvariant() == roleName)));
        }
    }
}
