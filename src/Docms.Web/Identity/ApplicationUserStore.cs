using Docms.Application.Commands;
using Docms.Infrastructure.Identity;
using Docms.Queries.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace Docms.Web.Identity
{
    public class ApplicationUserStore :
        IUserStore<ApplicationUser>,
        IUserPasswordStore<ApplicationUser>,
        IUserRoleStore<ApplicationUser>,
        IUserSecurityStampStore<ApplicationUser>
    {
        private static readonly RandomNumberGenerator _rng = RandomNumberGenerator.Create();

        private readonly IMediator _mediator;
        private readonly IUsersQueries _usersQueries;

        public ApplicationUserStore(IMediator mediator, IUsersQueries usersQueries)
        {
            _mediator = mediator;
            _usersQueries = usersQueries;
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

            var user = await _usersQueries.FindByIdAsync(userId, cancellationToken).ConfigureAwait(false);
            return await GetApplicationUserAsync(user, cancellationToken).ConfigureAwait(false);
        }

        public async Task<ApplicationUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            var user = await _usersQueries.FindByNameAsync(normalizedUserName, cancellationToken).ConfigureAwait(false);
            return await GetApplicationUserAsync(user, cancellationToken).ConfigureAwait(false);
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
            await _mediator.Send(new UpdateSecurityStampCommand()
            {
                UserId = user.Id,
                SecurityStamp = stamp
            }).ConfigureAwait(false);
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
            return Task.FromResult<IList<string>>(user.Roles);
        }

        public Task<bool> IsInRoleAsync(ApplicationUser user, string roleName, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.Roles.Any(r => r.ToUpperInvariant() == roleName));
        }

        public async Task<IList<ApplicationUser>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            var users = await _usersQueries.GetUsersInRoleAsync(roleName, cancellationToken);

            var appUsers = new List<ApplicationUser>();
            foreach (var user in users)
            {
                appUsers.Add(await GetApplicationUserAsync(user, cancellationToken).ConfigureAwait(false));
            }
            return appUsers;
        }

        private async Task<ApplicationUser> GetApplicationUserAsync(User user, CancellationToken cancellationToken)
        {
            var hasher = new PasswordHasher<ApplicationUser>();
            var appUser = new ApplicationUser()
            {
                Id = user.Id,
                Name = user.Name,
                AccountName = user.AccountName,
                DepartmentName = user.DepartmentName,
                TeamName = user.TeamName,
                SecurityStamp = user.SecurityStamp
            };
            appUser.PasswordHash = hasher.HashPassword(appUser, user.Password);
            appUser.Roles.AddRange(user.Roles);

            if (string.IsNullOrEmpty(appUser.SecurityStamp))
            {
                await SetSecurityStampAsync(appUser, NewSecurityStamp(), cancellationToken);
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
            _usersQueries.Dispose();
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
