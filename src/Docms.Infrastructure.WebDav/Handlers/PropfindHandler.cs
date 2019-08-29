using Docms.Infrastructure.WebDav.DataModel;
using Microsoft.AspNetCore.Http;
using System.Text;
using System.Threading.Tasks;

namespace Docms.Infrastructure.WebDav.Handlers
{
    public class PropfindHandler : IWebDavRequestHandler
    {
        private readonly IPropertyProvider _provider;

        public PropfindHandler(IPropertyProvider provider)
        {
            _provider = provider;
        }

        public Task HandleAsync(HttpContext context)
        {
            var data = Encoding.UTF8.GetBytes(@"<?xml version=""1.0"" encoding=""utf-8"" ?><D:prop xmlns:D=""DAV:""/>");
            context.Response.StatusCode = Constatns.Status200Ok;
            context.Response.Headers.Add("Content-Type", new[] { "application/xml" });
            context.Response.Body.Write(data, 0, data.Length);
            return Task.CompletedTask;
        }
    }
}
