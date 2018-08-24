using Docms.Infrastructure.Files;
using Docms.Web.Application.Commands;
using Docms.Web.Application.Queries.Documents;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Web;

namespace Docms.Web.Api.V1
{
    // [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/v1/files")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private readonly IFileStorage _storage;
        private readonly DocumentsQueries _queries;

        public FilesController(IFileStorage storage, DocumentsQueries queries)
        {
            _storage = storage;
            _queries = queries;
        }


        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] string path)
        {
            var entry = await _queries.GetEntryAsync(path ?? "");
            if (entry == null)
            {
                return NotFound();
            }
            return Ok(entry);
        }

        [HttpPost]
        public async Task<IActionResult> Post(
            [FromForm] UploadRequest request,
            [FromServices] IMediator mediator)
        {
            var filepath = new FilePath(request.Path);
            using (var stream = request.File.OpenReadStream())
            {
                var command = new CreateOrUpdateDocumentCommand();
                command.Path = filepath;
                command.Stream = stream;
                command.Created = request.Created;
                command.LastModified = request.LastModified;
                var response = await mediator.Send(command);
                return CreatedAtAction("Get", new { path = HttpUtility.UrlEncode(command.Path.ToString()) });
            }
        }

        [HttpPost("move")]
        public async Task<IActionResult> Move(
            [FromForm] MoveRequest request,
            [FromServices] IMediator mediator)
        {
            var originalFilePath = new FilePath(request.OriginalPath);
            var destinationFilePath = new FilePath(request.DestinationPath);
            var command = new MoveDocumentCommand();
            command.OriginalPath = originalFilePath;
            command.DestinationPath = destinationFilePath;
            var response = await mediator.Send(command);
            return CreatedAtAction("Get", new { path = HttpUtility.UrlEncode(command.DestinationPath.ToString()) });
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(
            [FromQuery] string path,
            [FromServices] IMediator mediator)
        {
            var filePath = new FilePath(path);
            var command = new DeleteDocumentCommand();
            command.Path = filePath;
            var response = await mediator.Send(command);
            return Ok();
        }
    }
}