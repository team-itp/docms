using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Docms.Infrastructure.WebDav.Handlers
{
    interface IWebDavRequestHandler
    {
        Task HandleAsync(HttpContext context);
    }
}
