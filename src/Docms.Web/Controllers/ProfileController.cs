using Docms.Web.Data;
using Docms.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
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

            var projectTags = await _context.Tags
                .Include(e => e.Metadata)
                .Where(e => e.Metadata.Any(m => m.MetaKey == "category"))
                .Where(e => e.Metadata.First(m => m.MetaKey == "category").MetaValue == "案件")
                .ToListAsync();

            projectTags = projectTags
                .OrderBy(e => int.TryParse(e["category_tag_order"], out var v) ? v : int.MaxValue)
                .ToList();
            ViewData["ProjectTags"] = projectTags;

            return View(new ProfileViewModel()
            {
                AccountName = appUser.AccountName,
                Name = appUser.Name,
                DepartmentName = appUser.DepartmentName,
                PreferredTags = (user?.UserPreferredTags
                    .Select(e => new PreferredTagViewModel()
                    {
                        Id = e.TagId,
                        Name = e.Tag.Name
                    }) ?? new List<PreferredTagViewModel>()).ToList(),
            });
        }

        [HttpPost("preferredtags/add")]
        public async Task<IActionResult> AddPreferredTag([Bind("tagId")] int tagId)
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
            if (user == null)
            {
                user = new User()
                {
                    VSUserId = appUser.Id
                };
                await _context.AddAsync(user);
                user = await _context.Users.FindAsync(user.Id);
            }
            user.AddPreferredTag(tag);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost("preferredtags/delete/{tagId}")]
        public async Task<IActionResult> DeletePreferredTag(int tagId)
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

            if (user != null)
            {
                user.RemovePreferredTag(tag);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
