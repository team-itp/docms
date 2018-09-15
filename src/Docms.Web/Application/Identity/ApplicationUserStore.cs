using Docms.Infrastructure;
using Docms.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using VisualizationSystem.Infrastructure;
using VisualizationSystem.Infrastructure.Models;

namespace Docms.Web.Application.Identity
{
    public class ApplicationUserStore :
        IUserStore<ApplicationUser>,
        IUserPasswordStore<ApplicationUser>,
        IUserRoleStore<ApplicationUser>,
        IUserSecurityStampStore<ApplicationUser>
    {
        private VisualizationSystemContext _vsDb;
        private DocmsContext _docmsDb;
        private static RandomNumberGenerator _rng = RandomNumberGenerator.Create();
        private static Users ADMIN_USER = new Users()
        {
            Id = "admin",
            Name = "管理者",
            AccountName = "admin",
            Password = "jEQE5hLa",
            Department = -1,
            TeamId = null,
        };

        public ApplicationUserStore(VisualizationSystemContext vsDb, DocmsContext docmsDb)
        {
            _vsDb = vsDb;
            _docmsDb = docmsDb;
        }

        public Task<IdentityResult> CreateAsync(ApplicationUser user, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task SetNormalizedUserNameAsync(ApplicationUser user, string normalizedName, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task SetUserNameAsync(ApplicationUser user, string userName, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task SetPasswordHashAsync(ApplicationUser user, string passwordHash, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<IdentityResult> UpdateAsync(ApplicationUser user, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<IdentityResult> DeleteAsync(ApplicationUser user, CancellationToken cancellationToken) => throw new System.NotImplementedException();

        public async Task<ApplicationUser> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (string.IsNullOrEmpty(userId)) throw new ArgumentNullException(nameof(userId));
            if (userId == ADMIN_USER.Id)
            {
                return await GetApplicationUserAsync(ADMIN_USER);
            }
            var user = await _vsDb.Users.FindAsync(userId);
            if (user == null)
            {
                return null;
            }
            return await GetApplicationUserAsync(user, cancellationToken);
        }

        public async Task<ApplicationUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (string.IsNullOrEmpty(normalizedUserName)) throw new ArgumentNullException(nameof(normalizedUserName));
            if (normalizedUserName == ADMIN_USER.AccountName.ToUpperInvariant())
            {
                return await GetApplicationUserAsync(ADMIN_USER);
            }

            var user = await _vsDb.Users.FirstOrDefaultAsync(u => u.AccountName.ToUpperInvariant() == normalizedUserName);
            if (user == null)
            {
                return null;
            }

            return await GetApplicationUserAsync(user, cancellationToken);
        }


        public Task<string> GetNormalizedUserNameAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null) throw new ArgumentNullException(nameof(user));
            return Task.FromResult(user.AccountName.ToUpperInvariant());
        }

        public Task<string> GetUserIdAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null) throw new ArgumentNullException(nameof(user));
            return Task.FromResult(user.Id);
        }

        public Task<string> GetUserNameAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null) throw new ArgumentNullException(nameof(user));
            return Task.FromResult(user.AccountName);
        }

        public Task<string> GetPasswordHashAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null) throw new ArgumentNullException(nameof(user));
            return Task.FromResult(user.PasswordHash);
        }

        public Task<bool> HasPasswordAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null) throw new ArgumentNullException(nameof(user));
            return Task.FromResult(!string.IsNullOrEmpty(user.PasswordHash));
        }

        public async Task SetSecurityStampAsync(ApplicationUser user, string stamp, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null) throw new ArgumentNullException(nameof(user));
            var docmsUser = await _docmsDb.Users.FirstOrDefaultAsync(e => e.Id == user.Id);
            if (docmsUser == null)
            {
                docmsUser = new DocmsUser()
                {
                    Id = user.Id,
                    SecurityStamp = stamp ?? throw new ArgumentNullException(nameof(stamp))
                };
                _docmsDb.Users.Add(docmsUser);
            }
            else
            {
                docmsUser.SecurityStamp = stamp ?? throw new ArgumentNullException(nameof(stamp));
                _docmsDb.Update(docmsUser);
            }
            await _docmsDb.SaveChangesAsync();
            user.SecurityStamp = stamp;
        }

        public Task<string> GetSecurityStampAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null) throw new ArgumentNullException(nameof(user));
            return Task.FromResult(user.SecurityStamp);
        }

        public Task AddToRoleAsync(ApplicationUser user, string roleName, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task RemoveFromRoleAsync(ApplicationUser user, string roleName, CancellationToken cancellationToken) => throw new NotImplementedException();

        public Task<IList<string>> GetRolesAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            if (user.AccountName == ADMIN_USER.AccountName)
            {
                return Task.FromResult<IList<string>>(new List<string>() {
                    "Administrator"
                });
            }
            return Task.FromResult<IList<string>>(new List<string>());
        }

        public Task<bool> IsInRoleAsync(ApplicationUser user, string roleName, CancellationToken cancellationToken)
        {
            if (user.AccountName == ADMIN_USER.AccountName)
            {
                return Task.FromResult(roleName == "Administrator".ToUpperInvariant());
            }
            return Task.FromResult(false);
        }

        public async Task<IList<ApplicationUser>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken)
        {
            if (roleName == "Administrator".ToUpperInvariant())
            {
                return new List<ApplicationUser>() {
                    await FindByNameAsync(ADMIN_USER.AccountName.ToUpperInvariant(), cancellationToken)
                };
            }
            return new List<ApplicationUser>();
        }

        private async Task<ApplicationUser> GetApplicationUserAsync(Users user, CancellationToken cancellationToken = default(CancellationToken))
        {
            var teamName = default(string);
            if (!string.IsNullOrEmpty(user.TeamId))
            {
                teamName = (await _vsDb.Teams.FindAsync(user.TeamId))?.Name;
            }

            var hasher = new PasswordHasher<ApplicationUser>();
            var appUser = new ApplicationUser()
            {
                Id = user.Id,
                Name = user.Name,
                AccountName = user.AccountName,
                DepartmentName = user.Department == 0
                                    ? "リフォーム"
                                    : user.Department == 1
                                    ? "建築"
                                    : null,
                TeamName = teamName,
            };
            appUser.PasswordHash = hasher.HashPassword(appUser, user.Password);

            var docmsUser = await _docmsDb.Users.FirstOrDefaultAsync(e => e.Id == user.Id);
            if (docmsUser == null)
            {
                await SetSecurityStampAsync(appUser, NewSecurityStamp(), cancellationToken);
            }
            else
            {
                appUser.SecurityStamp = docmsUser.SecurityStamp;
            }

            return appUser;
        }

        private string NewSecurityStamp()
        {
            byte[] bytes = new byte[20];
            _rng.GetBytes(bytes);
            return Base32.ToBase32(bytes);
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
