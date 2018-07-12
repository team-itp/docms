using Docms.Web.Data;
using Docms.Web.Models;
using Docms.Web.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Docms.Web.Controllers
{
    /// <summary>
    /// ドキュメント情報 API コントローラー
    /// </summary>
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Produces("application/json")]
    [Route("api/documents")]
    public class DocumentsApiController : Controller
    {
        private readonly DocmsDbContext _context;
        private readonly DocumentsService _service;

        public DocumentsApiController(DocmsDbContext context, DocumentsService service)
        {
            _context = context;
            _service = service;
        }

        /// <summary>
        /// 指定したドキュメント情報を取得する
        /// </summary>
        /// <param name="id">ドキュメントID</param>
        /// <returns>ドキュメント情報</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Document))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
        public async Task<IActionResult> GetDocument([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var document = await _context.Documents.SingleOrDefaultAsync(m => m.Id == id);

            if (document == null)
            {
                return NotFound();
            }

            return Ok(document);
        }

        /// <summary>
        /// ドキュメント情報を作成する
        /// </summary>
        /// <param name="document">ドキュメント情報</param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
        public async Task<IActionResult> PostDocument([FromBody] UploadDocumentRequest document)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userAccountName = User.Identity.Name;

            var documentId = await _service.CreateAsync(
                document.BlobName,
                document.Name,
                userAccountName,
                document.Tags.Where(e => !string.IsNullOrEmpty(e)),
                document.PersonInCharge,
                document.Customer, 
                document.Project);

            return CreatedAtAction("GetDocument", new { id = documentId });
        }
    }

    public class UploadDocumentRequest
    {
        public string BlobName { get; set; }
        public string Name { get; set; }
        public string PersonInCharge { get; set; }
        public string Customer { get; set; }
        public string Project { get; set; }
        public string[] Tags { get; set; } = new string[0];
    }
}