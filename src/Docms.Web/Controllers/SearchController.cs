using Docms.Web.Data;
using Docms.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Docms.Web.Controllers
{
    public class SearchController : Controller
    {
        private DocmsDbContext _context;

        public SearchController(DocmsDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// ドキュメントの検索
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Index([Bind("t")]string[] tags)
        {
            var documents = _context.Documents
                .Include(e => e.Tags)
                .ThenInclude(e => e.Tag)
                .AsQueryable();

            if (tags.Any())
            {
                documents = documents.Where(e => e.Tags.Any(t => tags.Contains(t.Tag.Name)));
            }

            return View(new SearchResultViewModel()
            {
                Results = documents.Select(d => new SearchResultItemViewModel()
                {
                    Id = d.Id,
                    BlobUri = Url.Action("Get", "Blobs", new { blobName = d.BlobName }),
                    ThumbnailUri = Url.Action("Get", "Blobs", new { blobName = d.BlobName }),
                    FileName = d.FileName,
                    UploadedAt = d.UploadedAt
                }).ToList()
            });
        }
    }
}