using Docms.Infrastructure.WebDav.Handlers;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Docms.Infrastructure.WebDav
{
    internal class WebDavContext
    {
        private HttpContext _context;
        private WebDavOptions _options;
        private IWebDavRequestHandler _handler;

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
                    _handler = new PropfindHandler(_options.PropertyStore);
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
            return _handler.HandleAsync(_context);
        }
    }
}
