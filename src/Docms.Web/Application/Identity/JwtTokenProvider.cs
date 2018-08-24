using IdentityServer4;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Docms.Web.Application.Identity
{
    public class JwtTokenProvider
    {
        private IdentityServerTools _identityServerTools;
        private IHttpContextAccessor _contextAccessor;
        private ITokenCreationService _tokenCreation;

        public JwtTokenProvider(
            IHttpContextAccessor contextAccessor,
            ITokenCreationService tokenCreation,
            ISystemClock clock)
        {
            _identityServerTools = new IdentityServerTools(contextAccessor, tokenCreation, clock);
            _contextAccessor = contextAccessor;
        }

        public async Task<string> GenerateToken()
        {
            return await _identityServerTools.IssueClientJwtAsync("docms-client", 60 * 60 * 24, new[] { "docmsapi" }, new[] { "docmsapi" });
        }
    }
}
