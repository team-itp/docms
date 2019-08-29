using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace Docms.Infrastructure.WebDav
{
    public class WebDavMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly WebDavOptions _options;

        public WebDavMiddleware(RequestDelegate next, WebDavOptions options)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public Task Invoke(HttpContext context)
        {
            var davContext = new WebDavContext(context, _options);
            if (davContext.ValidateMethod()
                && davContext.ValidatePath())
            {
                return davContext.ProcessRequestAsync();
            }

            return _next(context);
        }
    }
}