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
            var entry = await _blobs.GetEntryAsync(path);
            if (entry == null)
            {
                return Redirect(Url.ViewFile(path.Split("/").Last()));
            }
            ViewData["DirPath"] = entry is BlobContainer ? entry.Path : entry.ParentPath;
            return View(entry);
        }

        [HttpGet("histories/{*path=}")]
        public async Task<IActionResult> Histories(string path,
            [FromQuery] int? page = null,
            [FromQuery] int? per_page = null)
        {
            var entry = await _blobs.GetEntryAsync(path);
            if (entry == null)
            {
                return Redirect(Url.ViewFile(path.Split("/").Last()));
            }
            ViewData["DirPath"] = entry is BlobContainer ? entry.Path : entry.ParentPath;

            var histories = _histories.GetHistories(path ?? "")
                .Where(h => h.Discriminator == DocumentHistoryDiscriminator.DocumentCreated || h.Discriminator == DocumentHistoryDiscriminator.DocumentDeleted)
                .OrderByDescending(h => h.Timestamp);
            var cnt = await histories.CountAsync();
            int p = page ?? 1;
            int pp = per_page ?? 10;
            return View(
                new PageableList<DocumentHistory>(
                    await histories.Skip(pp * (p - 1)).Take(pp).ToListAsync(),
                    p, pp, cnt));
        }


        [HttpHead("download/{*path}")]
        public async Task<IActionResult> DownloadHead(string path)
        {
            if (!(await _blobs.GetEntryAsync(path) is Blob entry))
            {
                return NotFound();
            }

            var headers = Request.GetTypedHeaders();
            if (headers.IfNoneMatch != null
                && headers.IfNoneMatch.Contains(new EntityTagHeaderValue("\"" + entry.Hash + "\"")))
            {
                return new StatusCodeResult((int)HttpStatusCode.NotModified);
            }

            var data = await _storage.FindAsync(entry.StorageKey ?? entry.Path);
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
            if (!(await _blobs.GetEntryAsync(path) is Blob entry))
            {
                return NotFound();
            }

            var headers = Request.GetTypedHeaders();
            if (headers.IfNoneMatch != null
                && headers.IfNoneMatch.Contains(new EntityTagHeaderValue("\"" + entry.Hash + "\"")))
            {
                return new StatusCodeResult((int)HttpStatusCode.NotModified);
            }

            var data = await _storage.FindAsync(entry.StorageKey ?? entry.Path);
            if (data == null)
            {
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }

            Response.Headers.Add("Cache-Control", "max-age=15");
            return File(await data.OpenStreamAsync(), entry.ContentType, entry.Name, new DateTimeOffset(entry.LastModified), new EntityTagHeaderValue("\"" + entry.Hash + "\""), true);
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