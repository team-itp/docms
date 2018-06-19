using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Docms.Web.Data;

namespace Docms.Web.Controllers
{
    public class TagGroupTagsController : Controller
    {
        private readonly DocmsDbContext _context;

        public TagGroupTagsController(DocmsDbContext context)
        {
            _context = context;
        }

        // GET: TagGroupTags
        public async Task<IActionResult> Index()
        {
            var docmsDbContext = _context.TagGroupTags.Include(t => t.Tag).Include(t => t.TagGroup);
            return View(await docmsDbContext.ToListAsync());
        }

        // GET: TagGroupTags/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tagGroupTag = await _context.TagGroupTags
                .Include(t => t.Tag)
                .Include(t => t.TagGroup)
                .SingleOrDefaultAsync(m => m.TagGroupId == id);
            if (tagGroupTag == null)
            {
                return NotFound();
            }

            return View(tagGroupTag);
        }

        // GET: TagGroupTags/Create
        public IActionResult Create()
        {
            ViewData["TagId"] = new SelectList(_context.Tags, "Id", "Id");
            ViewData["TagGroupId"] = new SelectList(_context.TagGroups, "Id", "Id");
            return View();
        }

        // POST: TagGroupTags/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TagGroupId,TagId")] TagGroupTag tagGroupTag)
        {
            if (ModelState.IsValid)
            {
                _context.Add(tagGroupTag);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["TagId"] = new SelectList(_context.Tags, "Id", "Id", tagGroupTag.TagId);
            ViewData["TagGroupId"] = new SelectList(_context.TagGroups, "Id", "Id", tagGroupTag.TagGroupId);
            return View(tagGroupTag);
        }

        // GET: TagGroupTags/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tagGroupTag = await _context.TagGroupTags.SingleOrDefaultAsync(m => m.TagGroupId == id);
            if (tagGroupTag == null)
            {
                return NotFound();
            }
            ViewData["TagId"] = new SelectList(_context.Tags, "Id", "Id", tagGroupTag.TagId);
            ViewData["TagGroupId"] = new SelectList(_context.TagGroups, "Id", "Id", tagGroupTag.TagGroupId);
            return View(tagGroupTag);
        }

        // POST: TagGroupTags/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TagGroupId,TagId")] TagGroupTag tagGroupTag)
        {
            if (id != tagGroupTag.TagGroupId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(tagGroupTag);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TagGroupTagExists(tagGroupTag.TagGroupId))
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
            ViewData["TagId"] = new SelectList(_context.Tags, "Id", "Id", tagGroupTag.TagId);
            ViewData["TagGroupId"] = new SelectList(_context.TagGroups, "Id", "Id", tagGroupTag.TagGroupId);
            return View(tagGroupTag);
        }

        // GET: TagGroupTags/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tagGroupTag = await _context.TagGroupTags
                .Include(t => t.Tag)
                .Include(t => t.TagGroup)
                .SingleOrDefaultAsync(m => m.TagGroupId == id);
            if (tagGroupTag == null)
            {
                return NotFound();
            }

            return View(tagGroupTag);
        }

        // POST: TagGroupTags/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tagGroupTag = await _context.TagGroupTags.SingleOrDefaultAsync(m => m.TagGroupId == id);
            _context.TagGroupTags.Remove(tagGroupTag);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TagGroupTagExists(int id)
        {
            return _context.TagGroupTags.Any(e => e.TagGroupId == id);
        }
    }
}
