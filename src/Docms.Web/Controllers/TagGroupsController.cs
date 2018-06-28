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
    public class TagGroupsController : Controller
    {
        private readonly DocmsDbContext _context;

        public TagGroupsController(DocmsDbContext context)
        {
            _context = context;
        }

        // GET: TagGroups
        public async Task<IActionResult> Index()
        {
            return View(await _context.TagGroups.ToListAsync());
        }

        // GET: TagGroups/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tagGroup = await _context.TagGroups
                .SingleOrDefaultAsync(m => m.Id == id);
            if (tagGroup == null)
            {
                return NotFound();
            }

            return View(tagGroup);
        }

        // GET: TagGroups/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: TagGroups/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title")] TagGroup tagGroup)
        {
            if (ModelState.IsValid)
            {
                _context.Add(tagGroup);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(tagGroup);
        }

        // GET: TagGroups/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tagGroup = await _context.TagGroups.SingleOrDefaultAsync(m => m.Id == id);
            if (tagGroup == null)
            {
                return NotFound();
            }
            return View(tagGroup);
        }

        // POST: TagGroups/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title")] TagGroup tagGroup)
        {
            if (id != tagGroup.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(tagGroup);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TagGroupExists(tagGroup.Id))
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
            return View(tagGroup);
        }

        // GET: TagGroups/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tagGroup = await _context.TagGroups
                .SingleOrDefaultAsync(m => m.Id == id);
            if (tagGroup == null)
            {
                return NotFound();
            }

            return View(tagGroup);
        }

        // POST: TagGroups/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tagGroup = await _context.TagGroups.SingleOrDefaultAsync(m => m.Id == id);
            _context.TagGroups.Remove(tagGroup);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TagGroupExists(int id)
        {
            return _context.TagGroups.Any(e => e.Id == id);
        }
    }
}
