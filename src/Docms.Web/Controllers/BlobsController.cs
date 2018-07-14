using Docms.Web.Data;
using Docms.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;
using System.IO;
using System.Threading.Tasks;

namespace Docms.Web.Controllers
{
    /// <summary>
    /// ドキュメント取得 (AzureStorageのWrapper)
    /// </summary>
    [Authorize]
    [Route("blobs")]
    public class BlobsController : Controller
    {
        private IStorageService _storage;
        private DocmsDbContext _context;

        public BlobsController(IStorageService storage, DocmsDbContext context)
        {
            _storage = storage;
            _context = context;
        }

        [HttpGet("{blobName}")]
        public async Task<IActionResult> Get(string blobName)
        {
            var info = await _storage.GetBlobInfo(blobName);
            var doc = await _context.Documents.FirstOrDefaultAsync(d => d.BlobName == blobName);
            if (info == null || doc == null)
            {
                return NotFound();
            }

            if (Request.Headers.Keys.Contains("If-None-Match") && Request.Headers["If-None-Match"].ToString() == info.ETag)
            {
                return new StatusCodeResult(304);
            }

            var stream = await _storage.OpenStreamAsync(blobName);
            return File(stream, info.ContentType, doc.FileName, info.LastModified, new EntityTagHeaderValue(info.ETag));
        }

        [HttpGet("thumbnails/{blobName}_{size}")]
        public async Task<IActionResult> GetThumbnail(string blobName, string size)
        {
            var info = await _storage.GetThumbInfo(blobName, size);
            if (info == null)
            {
                return NotFound();
            }

            if (Request.Headers.Keys.Contains("If-None-Match") && Request.Headers["If-None-Match"].ToString() == info.ETag)
            {
                return new StatusCodeResult(304);
            }

            var stream = await _storage.OpenThumbnailStreamAsync(blobName, size);
            return File(stream, info.ContentType, info.LastModified, new EntityTagHeaderValue(info.ETag));
        }

        [HttpPost]
        [RequestSizeLimit(100_000_000)]
        public async Task<IActionResult> Post(IFormFile file)
        {
            new FileExtensionContentTypeProvider().TryGetContentType(file.FileName, out var contentType);
            var blobName = await _storage.UploadFileAsync(file.OpenReadStream(), contentType ?? "application/octet-stream");
            return CreatedAtAction(nameof(Get), new { blobName }, new { blobName });
        }
    }
}
