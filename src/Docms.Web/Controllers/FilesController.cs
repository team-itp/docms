using Docms.Web.Application.Queries;
using Docms.Web.Application.Queries.Documents;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Docms.Web.Controllers
{
    [Route("files")]
    public class FilesController : Controller
    {
        private DocumentsQueries _queries;

        public FilesController(DocumentsQueries queries)
        {
            _queries = queries;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var entry = await _queries.GetEntryAsync("");
            return View(entry);
        }

        [HttpGet("{*path}")]
        public async Task<IActionResult> Index(string path)
        {
            var entry = await _queries.GetEntryAsync(path);
            return View(entry);
        }
    }
}