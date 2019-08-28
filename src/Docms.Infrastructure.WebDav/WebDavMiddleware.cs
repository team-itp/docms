using Microsoft.AspNetCore.Http;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Docms.Infrastructure.WebDav
{
    public class WebDavMiddleware
    {
        private readonly RequestDelegate _next;

        public WebDavMiddleware(RequestDelegate next)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
        }

        public async Task Invoke(HttpContext httpContext)
        {
            switch (httpContext.Request.Method)
            {
                case "PROPFIND":
                    var data = Encoding.UTF8.GetBytes(@"<?xml version=""1.0"" encoding=""utf-8"" ?><D:prop xmlns:D=""DAV:""/>");
                    httpContext.Response.Headers.Add("Content-Type", "application/xml");
                    httpContext.Response.Body.Write(data, 0, data.Length);
                    return;
            }
            await _next(httpContext);
        }
    }
}