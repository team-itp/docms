using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Docms.Web.Data;

namespace Docms.Web.Controllers
{
    [Authorize]
    public class TagSearchTagsController : Controller
    {
        private readonly DocmsDbContext _context;

        public TagSearchTagsController(DocmsDbContext context)
        {
            _context = context;
        }

        // GET: TagSearchTags
        public async Task<IActionResult> Index()
        {
            var docmsDbContext = _context.TagSearchTags.Include(t => t.Category).Include(t => t.Tag);
            return View(await docmsDbContext.ToListAsync());
        }

        // GET: TagSearchTags/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tagSearchTag = await _context.TagSearchTags
                .Include(t => t.Category)
                .Include(t => t.Tag)
                .SingleOrDefaultAsync(m => m.TagSearchCategoryId == id);
            if (tagSearchTag == null)
            {
                return NotFound();
            }

            return View(tagSearchTag);
        }

        // GET: TagSearchTags/Create
        public IActionResult Create()
        {
            ViewData["TagSearchCategoryId"] = new SelectList(_context.TagSearchCategories, "Id", "Id");
            ViewData["TagId"] = new SelectList(_context.Tags, "Id", "Id");
            return View();
        }

        // POST: TagSearchTags/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TagSearchCategoryId,TagId,Seq")] TagSearchTag tagSearchTag)
        {
            if (ModelState.IsValid)
            {
                _context.Add(tagSearchTag);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["TagSearchCategoryId"] = new SelectList(_context.TagSearchCategories, "Id", "Id", tagSearchTag.TagSearchCategoryId);
            ViewData["TagId"] = new SelectList(_context.Tags, "Id", "Id", tagSearchTag.TagId);
            return View(tagSearchTag);
        }

        // GET: TagSearchTags/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tagSearchTag = await _context.TagSearchTags.SingleOrDefaultAsync(m => m.TagSearchCategoryId == id);
            if (tagSearchTag == null)
            {
                return NotFound();
            }
            ViewData["TagSearchCategoryId"] = new SelectList(_context.TagSearchCategories, "Id", "Id", tagSearchTag.TagSearchCategoryId);
            ViewData["TagId"] = new SelectList(_context.Tags, "Id", "Id", tagSearchTag.TagId);
            return View(tagSearchTag);
        }

        // POST: TagSearchTags/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TagSearchCategoryId,TagId,Seq")] TagSearchTag tagSearchTag)
        {
            if (id != tagSearchTag.TagSearchCategoryId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(tagSearchTag);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TagSearchTagExists(tagSearchTag.TagSearchCategoryId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["TagSearchCategoryId"] = new SelectList(_context.TagSearchCategories, "Id", "Id", tagSearchTag.TagSearchCategoryId);
            ViewData["TagId"] = new SelectList(_context.Tags, "Id", "Id", tagSearchTag.TagId);
            return View(tagSearchTag);
        }

        // GET: TagSearchTags/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tagSearchTag = await _context.TagSearchTags
                .Include(t => t.Category)
                .Include(t => t.Tag)
                .SingleOrDefaultAsync(m => m.TagSearchCategoryId == id);
            if (tagSearchTag == null)
            {
                return NotFound();
            }

            return View(tagSearchTag);
        }

        // POST: TagSearchTags/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tagSearchTag = await _context.TagSearchTags.SingleOrDefaultAsync(m => m.TagSearchCategoryId == id);
            _context.TagSearchTags.Remove(tagSearchTag);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TagSearchTagExists(int id)
        {
            return _context.TagSearchTags.Any(e => e.TagSearchCategoryId == id);
        }
    }
}
