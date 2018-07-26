using Docms.Web.Data;
using Docms.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Docms.Web.Controllers
{
    [Route("search")]
    [Authorize]
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
            var tagSelection = await FetchTagSelection();

            var documents = _context.Documents
                .Include(e => e.Tags)
                .ThenInclude(e => e.Tag)
                .AsQueryable();

            if (tags.Any())
            {
                var taggroups = new List<int[]>();
                foreach (var panel in tagSelection.Panels)
                {
                    taggroups.Add(tags.Intersect(panel.Tags.Select(t => t.Id)).ToArray());
                }
                foreach (var taggroup in taggroups)
                {
                    if (taggroup.Length > 0)
                    {
                        documents = documents.Where(d => d.Tags.Any(t => taggroup.Contains(t.TagId)));
                    }
                }
            }

            if (!string.IsNullOrEmpty(keyword))
            {
                documents = documents.Where(e => e.Tags
                    .Any(t => t.Tag.Name.Contains(keyword)) || e.FileName.Contains(keyword));
            }

            var totalPages = ((await documents.CountAsync()) + 1) / CONTENT_PER_PAGE;

            documents = documents.OrderByDescending(d => d.UploadedAt);

            if (page.HasValue)
            {
                documents = documents.Skip((page.Value - 1) * CONTENT_PER_PAGE);
            }

            documents = documents.Take(CONTENT_PER_PAGE);

            var searchTags = _context
                .Tags
                .Where(t => tags.Contains(t.Id))
                .ToList();

            return View(SearchResultViewModel.Create(Url, keyword, searchTags, page ?? 1, totalPages, await documents.ToListAsync()));
        }

        private async Task<TagSelectionsViewModel> FetchTagSelection()
        {
            var tags = await _context.Tags
                    .Include(e => e.Metadata)
                    .Where(e => e.Metadata.Any(m => m.MetaKey == "category"))
                    .ToListAsync();
            var tagSelection = new TagSelectionsViewModel()
            {
                Panels = tags.OrderBy(eg => int.TryParse(eg["category_order"], out var order) ? order : int.MaxValue)
                    .GroupBy(e => e["category"])
                    .Select(e => new TagSelection()
                    {
                        Title = e.Key,
                        Tags = e
                            .OrderBy(eg => int.TryParse(eg["category_tag_order"], out var order) ? order : int.MaxValue)
                            .ToList()
                    })
                    .ToList()
            };
            ViewData["TagSelections"] = tagSelection;
            return tagSelection;
        }
    }
}
