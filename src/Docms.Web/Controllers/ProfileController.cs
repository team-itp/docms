using Docms.Web.Data;
using Docms.Web.Models;
using Docms.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Docms.Web.Controllers
{
    /// <summary>
    /// ユーザーコントローラ
    /// </summary>
    [Authorize]
    [Route("profile")]
    public class ProfileController : Controller
    {
        private DocmsDbContext _context;
        private UserManager<ApplicationUser> _userManager;

        public ProfileController(DocmsDbContext context, UserManager<ApplicationUser> userManager)
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

            return View(new ProfileViewModel()
            {
                AccountName = appUser.AccountName,
                Name = appUser.Name,
                DepartmentName = appUser.DepartmentName,
                PreferredTags = user.UserPreferredTags
                    .Select(e => new PreferredTagViewModel() {
                        Id = e.TagId,
                        Name = e.Tag.Name
                    }).ToList(),
            });
        }

        [HttpPost("preferredtags/add")]
        public async Task<IActionResult> AddPreferredTag([Bind("TagId")] int tagId)
        {
            var tag = await _context.Tags.FirstOrDefaultAsync(e => e.Id == tagId);
            if (tag == null)
            {
                BadRequest();
            }

            var appUser = await _userManager.GetUserAsync(User);
            var user = await _context.Users
                .Include(e => e.Metadata)
                .Include(e => e.UserPreferredTags)
                .FirstOrDefaultAsync(e => e.VSUserId == appUser.Id);
            user.AddPreferredTag(tag);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost("preferredtags/delete")]
        public async Task<IActionResult> DeletePreferredTag([Bind("TagId")] int tagId)
        {
            var tag = await _context.Tags.FirstOrDefaultAsync(e => e.Id == tagId);
            if (tag == null)
            {
                BadRequest();
            }

            var appUser = await _userManager.GetUserAsync(User);
            var user = await _context.Users
                .Include(e => e.Metadata)
                .Include(e => e.UserPreferredTags)
                .FirstOrDefaultAsync(e => e.VSUserId == appUser.Id);
            user.RemovePreferredTag(tag);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
