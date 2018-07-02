using Docms.Web.Data;
using Docms.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Docms.Web.Controllers
{
    [Authorize]
    [Route("")]
    public class HomeController : Controller
    {
        private DocmsDbContext _context;
        private UserManager<ApplicationUser> _userManager;

        public HomeController(DocmsDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {

            var appUser = await _userManager.GetUserAsync(User);
            var user = await _context.Users
                .Include(e => e.Metadata)
                .Include(e => e.UserPreferredTags)
                .ThenInclude(e => e.Tag)
                .FirstOrDefaultAsync(e => e.VSUserId == appUser.Id);

            var preferredTags = user.UserPreferredTags
                .Select(p => new PreferredTagViewModel()
                {
                    Id = p.TagId,
                    Name = p.Tag.Name
                });

            var userTag = _context.Tags.FirstOrDefaultAsync(t => t.Name == appUser.Name);
            var documentsQuery = _context.Documents.Include(d => d.Tags) as IQueryable<Document>;
            if (userTag != null)
            {
                documentsQuery = documentsQuery.Where(d => d.Tags.Any(t => t.TagId == userTag.Id));
            }

            documentsQuery = documentsQuery.Union(_context.Documents
                .Include(d => d.Tags)
                .Where(d => d.Tags.Any(t => preferredTags.Any(p => p.Id == t.TagId))));

            var documents = await documentsQuery
                .OrderBy(d => d.UploadedAt)
                .Take(40)
                .ToListAsync();

            return View(HomeViewModel.Create(Url, documents, preferredTags));
        }

        [HttpGet("about")]
        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        [HttpGet("contact")]
        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
