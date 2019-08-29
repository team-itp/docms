using Microsoft.AspNetCore.Http;
using System.Text;
using System.Threading.Tasks;

namespace Docms.Infrastructure.WebDav
{
    internal class WebDavContext
    {
        private HttpContext _context;
        private WebDavOptions _options;
        private bool _isPropfind;

        public WebDavContext(HttpContext context, WebDavOptions options)
        {
            _context = context;
            _options = options;
        }

        public bool ValidateMethod()
        {
            switch (_context.Request.Method)
            {
                case "PROPFIND":
                    _isPropfind = true;
                    return true;
            }
            return false;
        }

        public bool ValidatePath()
        {
            return true;
        }

        public Task ProcessRequestAsync()
        {
            if (_isPropfind)
            {
                var data = Encoding.UTF8.GetBytes(@"<?xml version=""1.0"" encoding=""utf-8"" ?><D:prop xmlns:D=""DAV:""/>");
                _context.Response.StatusCode = Constatns.Status200Ok;
                _context.Response.Headers.Add("Content-Type", new[] { "application/xml" });
                _context.Response.Body.Write(data, 0, data.Length);
            }
            return Task.CompletedTask;
        }
    }
}
