using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Docms.Web.IdentityServer
{
    public static class IdentityServerExtension
    {
        public static void AddIdentityServer4(this IServiceCollection services)
        {
            // configure identity server with in-memory stores, keys, clients and scopes
            services.AddIdentityServer()
                .AddDeveloperSigningCredential()
                .AddInMemoryIdentityResources(Config.GetIdentityResources())
                .AddInMemoryApiResources(Config.GetApiResources())
                .AddInMemoryClients(Config.GetClients())
                .AddTestUsers(Config.GetUsers());
        }

        public static void UseIdentityServer4(this IApplicationBuilder app)
        {
            app.UseIdentityServer();
        }
    }
}
