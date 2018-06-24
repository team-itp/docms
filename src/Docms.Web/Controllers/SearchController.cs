using Docms.Web.Data;
using Docms.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Docms.Web.Controllers
{
    [Route("search")]
    public class SearchController : Controller
    {
        private const int CONTENT_PER_PAGE = 10;

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
        public async Task<ActionResult> Index(
            [FromQuery(Name = "t")]int[] tags,
            [FromQuery(Name = "q")]string keyword,
            [FromQuery(Name = "p")]int? page)
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

            var totalPages = ((await documents.CountAsync()) / CONTENT_PER_PAGE) + 1;

            documents = documents.OrderByDescending(d => d.UploadedAt);

            if (page.HasValue)
            {
                documents = documents.Skip((page.Value - 1) * CONTENT_PER_PAGE);
            }

            documents.Take(CONTENT_PER_PAGE);

            var searchTags = _context
                .Tags
                .Where(t => tags.Contains(t.Id))
                .ToList();

            return View(SearchResultViewModel.Create(Url, keyword, searchTags, page ?? 1, totalPages, await documents.ToListAsync()));
        }

        private void FetchTagSelection()
        {
            var tagSelection = new TagSelectionsViewModel()
            {
                Panels = _context.TagSearchCategories
                    .Include(e => e.Tags)
                    .ThenInclude(e => e.Tag)
                    .Where(e => e.Tags.Any())
                    .OrderBy(e => e.Seq)
                    .Select(tg => new TagSelection()
                    {
                        Title = tg.Name,
                        Tags = tg.Tags.OrderBy(tgt => tgt.Seq)
                            .Select(tgt => tgt.Tag)
                            .ToList()
                    })
                    .ToList()
            };
            ViewData["TagSelections"] = tagSelection;
        }
    }
}