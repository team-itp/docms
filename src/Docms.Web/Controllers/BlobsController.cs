using Docms.Web.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Threading.Tasks;

namespace Docms.Web.Controllers
{
    /// <summary>
    /// ドキュメント取得 (AzureStorageのWrapper)
    /// </summary>
    [Route("blobs")]
    public class BlobsController : Controller
    {
        private IStorageService _service;

        public BlobsController(IStorageService service)
        {
            _service = service;
        }

        [HttpGet("{blobName}")]
        public async Task<IActionResult> Get(string blobName)
        {
            var stream = await _service.OpenStreamAsync(blobName);
            string contentType;
            new FileExtensionContentTypeProvider().TryGetContentType(blobName, out contentType);
            return File(stream, contentType ?? "application/octet-stream", blobName);
        }

        [HttpGet("thumbnails/{blobName}")]
        public async Task<IActionResult> Thumbnail(string blobName)
        {
            var stream = await _service.OpenStreamAsync(blobName);
            string contentType;
            new FileExtensionContentTypeProvider().TryGetContentType(blobName, out contentType);
            return File(stream, contentType ?? "application/octet-stream", blobName);
        }

        [HttpPost]
        [RequestSizeLimit(100_000_000)]
        public async Task<IActionResult> Post(IFormFile file)
        {
            await _service.UploadFileAsync(file.OpenReadStream(), Path.GetExtension(file.FileName));
            return CreatedAtAction(nameof(Get), new { blobName = file.FileName }, new { blobName = file.FileName });
        }
    }
}
