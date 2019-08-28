using Microsoft.AspNetCore.Builder;

namespace Docms.Infrastructure.WebDav
{
    public static class WebDavExtensions
    {
        public static IApplicationBuilder UseWebDav(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<WebDavMiddleware>();
        }
    }
}
