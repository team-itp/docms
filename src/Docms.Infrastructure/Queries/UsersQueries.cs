using Docms.Queries.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VisualizationSystem.Infrastructure;
using VisualizationSystem.Infrastructure.Models;

namespace Docms.Infrastructure.Queries
{
    public class UsersQueries : IUsersQueries, IDisposable
    {
        private readonly VisualizationSystemContext _vsDb;
        private readonly DocmsContext _docmsDb;

        private static readonly User ADMIN_USER = new User()
        {
            Id = "admin",
            Name = "管理者",
            AccountName = "admin",
            Password = "05Jgx6Uh",
            DepartmentName = null,
            TeamName = null,
            Roles = new List<string>() { "Administrator" }
        };

        public UsersQueries(VisualizationSystemContext vsDb, DocmsContext docmsDb)
        {
            _vsDb = vsDb;
            _docmsDb = docmsDb;
        }

        public async Task<User> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            if (string.IsNullOrEmpty(userId)) throw new ArgumentNullException(nameof(userId));
            if (userId == ADMIN_USER.Id)
            {
                return await GetAdminUserAsync();
            }
            var user = await _vsDb.Users.FindAsync(userId);
            if (user == null)
            {
                return null;
            }
            return await GetUserAsync(user, cancellationToken);
        }

        public async Task<User> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (string.IsNullOrEmpty(normalizedUserName)) throw new ArgumentNullException(nameof(normalizedUserName));
            if (normalizedUserName == ADMIN_USER.AccountName.ToUpperInvariant())
            {
                return await GetAdminUserAsync();
            }

            var user = await _vsDb.Users.FirstOrDefaultAsync(u => u.AccountName.ToUpperInvariant() == normalizedUserName);
            if (user == null)
            {
                return null;
            }

            return await GetUserAsync(user, cancellationToken);
        }

        public async Task<IEnumerable<User>> GetUsersInRoleAsync(string roleName, CancellationToken cancelleationToken)
        {
            var users = new List<User>();

            var usersByIds = await _docmsDb.UserRoles
                .Where(ur => ur.Role.ToUpperInvariant() == roleName)
                .Select(ur => ur.UserId)
                .ToListAsync()
                .ConfigureAwait(false);
            foreach (var userId in usersByIds)
            {
                users.Add(await FindByIdAsync(userId, cancelleationToken));
            }

            if (ADMIN_USER.Roles.Any(r => r.ToUpperInvariant() == roleName))
            {
                users.Add(ADMIN_USER);
            }

            return users;
        }

        private async Task<User> GetUserAsync(Users vsUser, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            var teamName = default(string);
            if (!string.IsNullOrEmpty(vsUser.TeamId))
            {
                teamName = (await _vsDb.Teams.FindAsync(vsUser.TeamId))?.Name;
            }

            var docmsUser = await _docmsDb.Users.FindAsync(vsUser.Id);
            var appUser = new User()
            {
                Id = vsUser.Id,
                Name = vsUser.Name,
                AccountName = vsUser.AccountName,
                DepartmentName = vsUser.Department == 0
                                    ? "リフォーム"
                                    : vsUser.Department == 1
                                    ? "建築"
                                    : null,
                TeamName = teamName,
                SecurityStamp = docmsUser?.SecurityStamp,
                Password = vsUser.Password
            };

            var roles = await _docmsDb
                .UserRoles
                .Where(u => u.UserId == vsUser.Id)
                .Select(u => u.Role)
                .ToListAsync()
                .ConfigureAwait(false);
            appUser.Roles = roles;

            return appUser;
        }

        private async Task<User> GetAdminUserAsync()
        {
            var docmsUser = await _docmsDb.Users.FindAsync(ADMIN_USER.Id);
            ADMIN_USER.SecurityStamp = docmsUser?.SecurityStamp;
            return ADMIN_USER;
        }

        public void Dispose()
        {
            _disposed = true;
            _vsDb.Dispose();
            _docmsDb.Dispose();
        }

        protected void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }

        private bool _disposed;
    }
}
