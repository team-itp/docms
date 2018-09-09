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
        private readonly IFileStorage _storage;
        private readonly IBlobsQueries _queries;

        public FilesController(IFileStorage storage, IBlobsQueries queries)
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

                var file = await _storage.GetEntryAsync(path) as File;
                return File(await file.OpenAsync(), blob.ContentType, entry.Name, new DateTimeOffset(blob.LastModified), new EntityTagHeaderValue("\"" + blob.Hash + "\""));
            }
            else
            {
                return Ok(entry);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post(
            [FromForm] UploadRequest request,
            [FromServices] IMediator mediator)
        {
            var filepath = new FilePath(request.Path);
            using (var stream = request.File.OpenReadStream())
            {
                var command = new CreateOrUpdateDocumentCommand
                {
                    Path = filepath,
                    Stream = stream,
                    Created = request.Created,
                    LastModified = request.LastModified
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
            var originalFilePath = new FilePath(request.OriginalPath);
            var destinationFilePath = new FilePath(request.DestinationPath);
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
            try
            {
                var filePath = new FilePath(path);
                var command = new DeleteDocumentCommand
                {
                    Path = filePath
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