using Docms.Web.Data;
using Docms.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
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
                .Include(e => e.UserFavorites)
                .FirstOrDefaultAsync(e => e.VSUserId == appUser.Id);

            var favoriteTags = new List<FavoriteTagViewModel>();
            if (user != null)
            {
                favoriteTags = user.UserFavorites
                    .OfType<UserFavoriteTag>()
                    .Select(p =>
                    {
                         _context.Entry(p).Reference(e => e.Tag).Load();
                        return new FavoriteTagViewModel()
                        {
                            Id = p.DataId,
                            Name = p.Tag.Name
                        };
                    }).ToList();
            }

            var userTag = _context.Tags.FirstOrDefaultAsync(t => t.Name == appUser.Name);
            var documentsQuery = _context.Documents.Include(d => d.Tags) as IQueryable<Document>;
            if (userTag != null)
            {
                documentsQuery = documentsQuery.Where(d => d.Tags.Any(t => t.TagId == userTag.Id));
            }

            documentsQuery = documentsQuery.Union(_context.Documents
                .Include(d => d.Tags)
                .Where(d => d.Tags.Any(t => favoriteTags.Any(p => p.Id == t.TagId))));

            var documents = await documentsQuery
                .OrderBy(d => d.UploadedAt)
                .Take(40)
                .ToListAsync();

            return View(HomeViewModel.Create(Url, documents, favoriteTags));
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
