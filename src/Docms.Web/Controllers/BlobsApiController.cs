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
    /// ドキュメント取得(API) (AzureStorageのWrapper)
    /// </summary>
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/blobs")]
    public class BlobsApiController : Controller
    {
        private IStorageService _service;

        public BlobsApiController(IStorageService service)
        {
            _service = service;
        }

        [HttpPost]
        [RequestSizeLimit(100_000_000)]
        public async Task<IActionResult> Post(IFormFile file)
        {
            new FileExtensionContentTypeProvider().TryGetContentType(file.FileName, out var contentType);
            var blobName = await _service.UploadFileAsync(file.OpenReadStream(), contentType ?? "application/octet-stream");
            return Ok(new { blobName });
        }
    }
}
