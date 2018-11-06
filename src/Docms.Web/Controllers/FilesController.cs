using Docms.Domain.Documents;
using Docms.Infrastructure.Files;
using Docms.Queries.Blobs;
using Docms.Web.Application.Commands;
using Docms.Web.Extensions;
using Docms.Web.Filters;
using Docms.Web.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Docms.Web.Controllers
{
    [Authorize]
    [DeviceIdentificationFilter]
    [Route("files")]
    public class FilesController : Controller
    {
        private readonly IDataStore _storage;
        private readonly IBlobsQueries _queries;

        public FilesController(IDataStore storage, IBlobsQueries queries)
        {
            _storage = storage;
            _queries = queries;
        }

        [HttpGet("view/{*path=}")]
        public async Task<IActionResult> Index(string path)
        {
            var entry = await _queries.GetEntryAsync(path);
            if (entry == null)
            {
                return NotFound();
            }
            ViewData["DirPath"] = entry is BlobContainer ? entry.Path : entry.ParentPath;
            return View(entry);
        }


        [HttpGet("download/{*path}")]
        public async Task<IActionResult> Download(string path)
        {
            if (!(await _queries.GetEntryAsync(path) is Blob entry))
            {
                return NotFound();
            }

            if (Request.Headers.Keys.Contains("If-None-Match") && Request.Headers["If-None-Match"].ToString() == "\"" + entry.Hash + "\"")
            {
                return new StatusCodeResult((int)HttpStatusCode.NotModified);
            }

            var data = await _storage.FindAsync(entry.StorageKey ?? entry.Path);
            if (data == null)
            {
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }

            Response.Headers.Add("Cache-Control", "max-age=15");
            return File(await data.OpenStreamAsync(), entry.ContentType, entry.Name, new DateTimeOffset(entry.LastModified), new EntityTagHeaderValue("\"" + entry.Hash + "\""));
        }

        [HttpGet("upload/{*dirPath=}")]
        public IActionResult Upload(string dirPath)
        {
            ViewData["DirPath"] = dirPath;
            return View(new UploadRequest()
            {
                DirPath = dirPath,
            });
        }

        [HttpPost("upload/{*dirPath=}")]
        public async Task<IActionResult> Upload(
            string dirPath,
            [FromForm] UploadRequest request,
            [FromServices] IMediator mediator)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            ViewData["DirPath"] = dirPath;
            var directryPath = new FilePath(dirPath ?? "");
            foreach (var file in request.Files)
            {
                var filePath = directryPath.Combine(System.IO.Path.GetFileName(file.FileName ?? "NONAME"));
                using (var stream = file.OpenReadStream())
                {
                    var command = new CreateOrUpdateDocumentCommand
                    {
                        Path = filePath,
                        Stream = stream,
                        SizeOfStream = file.Length,
                        ForceCreate = true
                    };
                    var response = await mediator.Send(command);
                }
            }
            return Redirect(Url.ViewFile(directryPath.ToString()));
        }
    }
}