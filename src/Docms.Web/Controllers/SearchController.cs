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
        /// ドキュメントの検索(キーワード検索)
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Index([Bind("t")]int[] tags, [Bind("q")]string keyword)
        {
            FetchTagSelection();

            var documents = _context.Documents
                .Include(e => e.Tags)
                .ThenInclude(e => e.Tag)
                .AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                documents = documents.Where(e => e.Tags.Any(t => t.Tag.Name.Contains(keyword)));
            }

            if (tags.Any())
            {
                documents = documents.Where(e => e.Tags.Any(t => tags.Contains(t.TagId)));
            }

            var searchTags = _context.Tags.Where(t => tags.Contains(t.Id));

            return View(new SearchResultViewModel()
            {
                SearchKeyword = keyword,
                SearchTags = searchTags,
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

        private void FetchTagSelection()
        {
            var tagSelection = new TagSelectionsViewModel()
            {
                Panels = _context.TagGroups
                    .Include(e => e.Tags)
                    .ThenInclude(e => e.Tag)
                    .Where(e => e.Tags.Any())
                    .Select(tg => new TagSelection()
                    {
                        Title = tg.Title,
                        Tags = tg.Tags.Select(tgt=> tgt.Tag).ToList()
                    })
                    .ToList()
            };
            ViewData["TagSelections"] = tagSelection;
        }
    }
}