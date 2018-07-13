using Docms.Web.Data;
using Docms.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
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
                .Include(e => e.UserFavorites)
                .FirstOrDefaultAsync(e => e.VSUserId == appUser.Id);

            var projectTags = await _context.Tags
                .Include(e => e.Metadata)
                .Where(e => e.Metadata.Any(m => m.MetaKey == Constants.TAG_KEY_CATEGORY))
                .Where(e => e.Metadata.First(m => m.MetaKey == Constants.TAG_KEY_CATEGORY).MetaValue == Constants.TAG_CATEGORY_PROJECT)
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
                TeamName = appUser.TeamName,
                Favorites = (user?.UserFavorites
                    .OfType<UserFavoriteTag>()
                    .Select(p =>
                    {
                        _context.Entry(p).Reference(e => e.Tag).Load();
                        return new FavoriteTagViewModel()
                        {
                            Id = p.Id,
                            TagId = p.Tag.Id,
                            Name = p.Tag.Name,
                        };
                    }) ?? new List<FavoriteTagViewModel>()).ToList(),
            });
        }

        [HttpPost("favorites/add")]
        public async Task<IActionResult> AddFavorites([Bind("Type,DataId,ReturnUrl")] AddFavoritesViewModel data)
        {
            if (data.Type != Constants.FAV_TYPE_TAG)
            {
                return BadRequest();
            }

            var tag = await _context.Tags.FirstOrDefaultAsync(e => e.Id == data.DataId);
            if (tag == null)
            {
                return BadRequest();
            }

            var user = await FindUserAsync();
            if (user == null)
            {
                var appUser = await _userManager.GetUserAsync(User);
                user = new User()
                {
                    VSUserId = appUser.Id
                };
                await _context.AddAsync(user);
                await _context.SaveChangesAsync();

                user = await FindUserAsync();
            }
            user.AddFavorites(tag);
            await _context.SaveChangesAsync();

            if (string.IsNullOrEmpty(data.ReturnUrl))
            {
                return RedirectToAction(nameof(Index));
            }
            else
            {
                return Redirect(data.ReturnUrl);
            }
        }

        [HttpPost("favorites/delete/{favId}")]
        public async Task<IActionResult> DeleteFavorites(int favId, [FromForm] string ReturnUrl)
        {
            var user = await FindUserAsync();
            if (user != null)
            {
                var fav = user.UserFavorites.FirstOrDefault(e => e.Id == favId);
                _context.Remove(fav);
                await _context.SaveChangesAsync();
            }

            if (string.IsNullOrEmpty(ReturnUrl))
            {
                return RedirectToAction(nameof(Index));
            }
            else
            {
                return Redirect(ReturnUrl);
            }
        }

        private async Task<User> FindUserAsync()
        {
            var appUser = await _userManager.GetUserAsync(User);
            return await _context.Users
                .Include(e => e.Metadata)
                .Include(e => e.UserFavorites)
                .FirstOrDefaultAsync(e => e.VSUserId == appUser.Id);
        }
    }
}
