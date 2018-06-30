using Docms.Web.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using System.IO;
using System.Threading.Tasks;

namespace Docms.Web.Controllers
{
    /// <summary>
    /// ドキュメント取得 (AzureStorageのWrapper)
    /// </summary>
    [Authorize]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
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
            new FileExtensionContentTypeProvider().TryGetContentType(blobName, out var contentType);
            return File(stream, contentType ?? "application/octet-stream", blobName);
        }

        [HttpGet("thumbnails/{blobName}")]
        public async Task<IActionResult> Thumbnail(string blobName)
        {
            var stream = await _service.OpenStreamAsync(blobName);
            new FileExtensionContentTypeProvider().TryGetContentType(blobName, out var contentType);
            return File(stream, contentType ?? "application/octet-stream", blobName);
        }

        [HttpPost]
        [RequestSizeLimit(100_000_000)]
        public async Task<IActionResult> Post(IFormFile file)
        {
            var blobName = await _service.UploadFileAsync(file.OpenReadStream(), Path.GetExtension(file.FileName));
            return CreatedAtAction(nameof(Get), new { blobName }, new { blobName });
        }
    }
}
