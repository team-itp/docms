using Docms.Queries.DocumentHistories;
using Docms.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Docms.Web.Controllers
{
    public class FileHistoriesController : Controller
    {
        private readonly IDocumentHistoriesQueries _queries;

        public FileHistoriesController(IDocumentHistoriesQueries queries)
        {
            _queries = queries;
        }

        public async Task<IActionResult> Histories(
            [FromQuery] string path,
            [FromQuery] DateTime? since = null,
            [FromQuery] int? page = null,
            [FromQuery] int? per_page = null)
        {
            var histories = _queries.GetHistories(path ?? "", since)
                .Where(h => h.Discriminator == DocumentHistoryDiscriminator.DocumentCreated
                    || h.Discriminator == DocumentHistoryDiscriminator.DocumentDeleted)
                .OrderByDescending(h => h.Timestamp);
            var cnt = await histories.CountAsync();
            int p = page ?? 1;
            int pp = per_page ?? 10;
            return View(
                new PageableList<DocumentHistory>(
                    await histories.Skip(pp * (p - 1)).Take(pp).ToListAsync(),
                    p, pp, cnt));
        }
    }
}
