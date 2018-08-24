using Docms.Infrastructure.Files;
using Docms.Web.Application.Commands;
using Docms.Web.Application.Queries.Documents;
using Docms.Web.Extensions;
using Docms.Web.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Docms.Web.Controllers
{
    [Authorize]
    [Route("files")]
    public class FilesController : Controller
    {
        private readonly IFileStorage _storage;
        private readonly DocumentsQueries _queries;

        public FilesController(IFileStorage storage, DocumentsQueries queries)
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
            ViewData["Path"] = path;
            return View(entry);
        }


        [HttpGet("download/{*path}")]
        public async Task<IActionResult> Download(string path)
        {
            var entry = await _queries.GetEntryAsync(path) as Document;
            if (entry == null)
            {
                return NotFound();
            }

            if (Request.Headers.Keys.Contains("If-None-Match") && Request.Headers["If-None-Match"].ToString() == "\"" + entry.Hash + "\"")
            {
                return new StatusCodeResult(304);
            }

            var file = await _storage.GetEntryAsync(path) as File;
            return File(await file.OpenAsync(), entry.ContentType, entry.Name, new DateTimeOffset(entry.LastModified), new EntityTagHeaderValue("\"" + entry.Hash + "\""));
        }

        [HttpGet("upload/{*dirPath=}")]
        public IActionResult Upload(string dirPath)
        {
            ViewData["DirPath"] = dirPath;
            return View();
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

            var filepath = new FilePath(dirPath, System.IO.Path.GetFileName(request.File.FileName ?? "NONAME"));
            using (var stream = request.File.OpenReadStream())
            {
                var command = new CreateOrUpdateDocumentCommand();
                command.Path = filepath;
                command.Stream = stream;
                command.ForceCreate = true;
                var response = await mediator.Send(command);
                return Redirect(Url.ViewFile(command.Path.DirectoryPath?.ToString()));
            }
        }
    }
}