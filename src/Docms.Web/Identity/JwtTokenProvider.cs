using IdentityServer4;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Docms.Web.Identity
{
    public class JwtTokenProvider
    {
        private readonly IdentityServerTools _identityServerTools;

        public JwtTokenProvider(
            IHttpContextAccessor contextAccessor,
            ITokenCreationService tokenCreation,
            ISystemClock clock)
        {
            _identityServerTools = new IdentityServerTools(contextAccessor, tokenCreation, clock);
        }

        public async Task<string> GenerateToken()
        {
            return await _identityServerTools.IssueClientJwtAsync("docms-client", 60 * 60 * 24, new[] { "docmsapi" }, new[] { "docmsapi" });
        }
    }
}
