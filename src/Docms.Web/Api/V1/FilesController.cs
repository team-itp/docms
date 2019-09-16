using Docms.Domain.Documents;
using Docms.Infrastructure.Files;
using Docms.Queries.Blobs;
using Docms.Web.Application.Commands;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using System;
using System.Threading.Tasks;
using System.Web;

namespace Docms.Web.Api.V1
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/v1/files")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private readonly IBlobsQueries _queries;
        private readonly IDataStore _storage;

        public FilesController(IDataStore storage, IBlobsQueries queries)
        {
            _storage = storage;
            _queries = queries;
        }


        [HttpGet]
        public async Task<IActionResult> Get(
            [FromQuery] string path = "",
            [FromQuery] bool download = false)
        {
            var entry = await _queries.GetEntryAsync(path ?? "");
            if (entry == null)
            {
                return NotFound();
            }
            if (download && entry is Blob blob)
            {
                if (Request.Headers.Keys.Contains("If-None-Match") && Request.Headers["If-None-Match"].ToString() == "\"" + blob.Hash + "\"")
                {
                    return new StatusCodeResult(304);
                }

                var data = await _storage.FindAsync(blob.StorageKey ?? blob.Path);
                return File(await data.OpenStreamAsync(), blob.ContentType, blob.Name, new DateTimeOffset(blob.LastModified), new EntityTagHeaderValue("\"" + blob.Hash + "\""));
            }
            else
            {
                return Ok(entry);
            }
        }

        [HttpPost]
        [RequestFormLimits(MultipartBodyLengthLimit = 4_294_967_294)] // 4GB
        public async Task<IActionResult> Post(
            [FromForm] UploadRequest request,
            [FromServices] IMediator mediator)
        {
            if (!FilePath.TryParse(request.Path, out var filepath)) {
                return BadRequest("invalid path name.");
            }

            using (var stream = request.File.OpenReadStream())
            {
                var command = new CreateOrUpdateDocumentCommand
                {
                    Path = filepath,
                    Stream = stream,
                    SizeOfStream = request.File.Length,
                    Created = request.Created?.ToUniversalTime(),
                    LastModified = request.LastModified?.ToUniversalTime()
                };
                var response = await mediator.Send(command);
                return CreatedAtAction("Get", new { path = HttpUtility.UrlEncode(command.Path.ToString()) });
            }
        }

        [HttpPost("move")]
        public async Task<IActionResult> Move(
            [FromForm] MoveRequest request,
            [FromServices] IMediator mediator)
        {
            if (!FilePath.TryParse(request.OriginalPath, out var originalFilePath)
                || !FilePath.TryParse(request.DestinationPath, out var destinationFilePath))
            {
                return BadRequest("invalid path name.");
            }

            var command = new MoveDocumentCommand
            {
                OriginalPath = originalFilePath,
                DestinationPath = destinationFilePath
            };
            var response = await mediator.Send(command);
            return CreatedAtAction("Get", new { path = HttpUtility.UrlEncode(command.DestinationPath.ToString()) });
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(
            [FromQuery] string path,
            [FromServices] IMediator mediator)
        {
            if (!FilePath.TryParse(path, out var filepath))
            {
                return BadRequest("invalid path name.");
            }

            try
            {
                var command = new DeleteDocumentCommand
                {
                    Path = filepath
                };
                var response = await mediator.Send(command);
                return Ok();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}