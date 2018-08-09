using Docms.Web.Application.Commands;
using Docms.Web.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading.Tasks;

namespace Docms.Web.Controllers
{
    [Route("docs")]
    public class DocumentsController : Controller
    {

        [HttpGet("upload")]
        public IActionResult Upload()
        {
            return View();
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload(
            [FromServices] IMediator mediator,
            [FromForm] UploadRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            using (var ms = new MemoryStream())
            {
                await request.File.OpenReadStream().CopyToAsync(ms);
                ms.Seek(0, SeekOrigin.Begin);
                var command = new CreateDocumentCommand();
                command.Path = request.Path;
                command.Stream = ms;
                var response = await mediator.Send(command);
                return View(response);
            }
        }
    }
}
