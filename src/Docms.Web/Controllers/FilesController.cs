using Docms.Domain.Documents;
using Docms.Infrastructure.Files;
using Docms.Queries.Blobs;
using Docms.Queries.DocumentHistories;
using Docms.Web.Application.Commands;
using Docms.Web.Extensions;
using Docms.Web.Filters;
using Docms.Web.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;
using System;
using System.Globalization;
using System.Linq;
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
        private readonly IBlobsQueries _blobs;
        private readonly IDocumentHistoriesQueries _histories;

        public FilesController(IDataStore storage, IBlobsQueries blobs, IDocumentHistoriesQueries histories)
        {
            _storage = storage;
            _blobs = blobs;
            _histories = histories;
        }

        [HttpGet("view/{*path=}")]
        public async Task<IActionResult> Index(string path)
        {
            var entry = await _blobs.GetEntryAsync(path).ConfigureAwait(false);
            if (entry == null)
            {
                return Redirect(Url.ViewFile(new DocumentPath(path).Parent?.Value));
            }
            ViewData["Path"] = entry.Path;
            return View(entry);
        }

        [HttpGet("histories/{*path=}")]
        public async Task<IActionResult> Histories(string path,
            [FromQuery] int page = 1,
            [FromQuery] int per_page = 100)
        {
            var entry = await _blobs.GetEntryAsync(path);
            if (entry == null)
            {
                return Redirect(Url.ViewFile(path.Split("/").Last()));
            }
            ViewData["Path"] = entry.Path;
            var histories = _histories.GetHistories(path ?? "")
                .OrderByDescending(d => d.Timestamp)
                .ThenByDescending(d => d.Path);
            var cnt = await histories.CountAsync().ConfigureAwait(false);
            var data = await histories.Skip(per_page * (page - 1)).Take(per_page).ToListAsync().ConfigureAwait(false);
            return View(
                new PageableList<DocumentHistory>(
                    data,
                    page, per_page, cnt));
        }


        [HttpHead("download/{*path}")]
        public async Task<IActionResult> DownloadHead(string path)
        {
            if (!(await _blobs.GetEntryAsync(path).ConfigureAwait(false) is Blob entry))
            {
                return NotFound();
            }

            var headers = Request.GetTypedHeaders();
            if (headers.IfNoneMatch != null
                && headers.IfNoneMatch.Contains(new EntityTagHeaderValue("\"" + entry.Hash + "\"")))
            {
                return new StatusCodeResult((int)HttpStatusCode.NotModified);
            }

            var data = await _storage.FindAsync(entry.StorageKey ?? entry.Path).ConfigureAwait(false);
            if (data == null)
            {
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }

            Response.Headers.Add("Last-Modified", entry.LastModified.ToString("ddd, dd MMM yyyy HH:mm:ss", CultureInfo.InvariantCulture) + " GMT");
            Response.Headers.Add("ETag", $"\"{entry.Hash}\"");
            Response.Headers.Add("Accept-Ranges", "bytes");
            Response.Headers.Add("Content-Length", entry.FileSize.ToString());
            Response.Headers.Add("Conent-Type", entry.ContentType);
            return Ok();
        }


        [HttpGet("download/{*path}")]
        public async Task<IActionResult> Download(string path)
        {
            if (!(await _blobs.GetEntryAsync(path).ConfigureAwait(false) is Blob entry))
            {
                return NotFound();
            }

            var headers = Request.GetTypedHeaders();
            if (headers.IfNoneMatch != null
                && headers.IfNoneMatch.Contains(new EntityTagHeaderValue("\"" + entry.Hash + "\"")))
            {
                return new StatusCodeResult((int)HttpStatusCode.NotModified);
            }

            var data = await _storage.FindAsync(entry.StorageKey ?? entry.Path).ConfigureAwait(false);
            if (data == null)
            {
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }

            Response.Headers.Add("Cache-Control", "max-age=15");
            return File(await data.OpenStreamAsync().ConfigureAwait(false), entry.ContentType, entry.Name, new DateTimeOffset(entry.LastModified), new EntityTagHeaderValue("\"" + entry.Hash + "\""), true);
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
                using (var stream = file.OpenReadStream())
                {
                    var data = await _storage.CreateAsync(_storage.CreateKey(), stream).ConfigureAwait(false);
                    var filePath = directryPath.Combine(System.IO.Path.GetFileName(file.FileName ?? "NONAME"));
                    var command = new CreateOrUpdateDocumentCommand
                    {
                        Path = filePath,
                        Data = data,
                        ForceCreate = true
                    };
                    var response = await mediator.Send(command).ConfigureAwait(false);
                }
            }
            return Redirect(Url.ViewFile(directryPath.ToString()));
        }
    }
}