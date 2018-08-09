using Docms.Infrastructure.Files;
using Docms.Web.Application.Queries.Documents;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using System;
using System.Threading.Tasks;

namespace Docms.Web.Controllers
{
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

        [HttpGet("view")]
        public async Task<IActionResult> Index()
        {
            var entry = await _queries.GetEntryAsync("");
            return View(entry);
        }


        [HttpGet("view/{*path}")]
        public async Task<IActionResult> Index(string path)
        {
            var entry = await _queries.GetEntryAsync(path);
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
    }
}