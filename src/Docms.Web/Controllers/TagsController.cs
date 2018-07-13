using Docms.Web.Data;
using Docms.Web.Models;
using Docms.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Docms.Web.Controllers
{
    /// <summary>
    /// タグコントローラー
    /// </summary>
    [Authorize]
    [Route("tags")]
    public class TagsController : Controller
    {
        private readonly DocmsDbContext _context;
        private readonly TagsService _service;

        public TagsController(DocmsDbContext context, TagsService service)
        {
            _context = context;
            _service = service;
        }

        /// <summary>
        /// タグの一覧を表示する
        /// </summary>
        /// <returns>ビューリザルト</returns>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            return View(await _context.Tags.ToListAsync());
        }

        /// <summary>
        /// タグの詳細を表示する
        /// </summary>
        /// <returns>ビューリザルト</returns>
        [HttpGet("details/{id}")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tag = await _context.Tags
                .Include(m => m.Metadata)
                .SingleOrDefaultAsync(m => m.Id == id);

            tag.Metadata = tag.Metadata.OrderBy(m => m.MetaKey).ToList();

            if (tag == null)
            {
                return NotFound();
            }

            return View(tag);
        }

        /// <summary>
        /// タグを新たに作成する
        /// </summary>
        /// <returns>ビューリザルト</returns>
        [HttpGet("create")]
        public IActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// タグを新たに作成する (Post)
        /// </summary>
        /// <param name="tag">タグ</param>
        /// <returns>ビューリザルト</returns>
        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name")] Tag tag)
        {
            if (ModelState.IsValid)
            {
                _context.Add(tag);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(tag);
        }

        /// <summary>
        /// タグを編集する
        /// </summary>
        /// <param name="id">タグID</param>
        /// <returns>ビューリザルト</returns>
        [HttpGet("edit/{id}/name")]
        public async Task<IActionResult> EditName(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tag = await _context.Tags.SingleOrDefaultAsync(m => m.Id == id);
            if (tag == null)
            {
                return NotFound();
            }
            return View(new EditTagNameViewModel()
            {
                Id = tag.Id,
                Name = tag.Name
            });
        }

        /// <summary>
        /// タグを編集する (Post)
        /// </summary>
        /// <param name="id">タグID</param>
        /// <param name="model">タグ</param>
        /// <returns>ビューリザルト</returns>
        [HttpPost("edit/{id}/name")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditName(int id, [Bind("Id,EditedName")] EditTagNameViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var tag = _context.Tags.Find(id);
                    tag.Name = model.EditedName;
                    _context.Update(tag);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TagExists(model.Id))
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
            return View(model);
        }


        /// <summary>
        /// タグのメタデータを新たに作成する
        /// </summary>
        /// <returns>ビューリザルト</returns>
        [HttpGet("create/{id}/metadata")]
        public async Task<IActionResult> CreateMetadata(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tag = await _context.Tags.SingleOrDefaultAsync(m => m.Id == id);
            if (tag == null)
            {
                return NotFound();
            }

            return View(new CreateTagMetaViewModel()
            {
                Id = tag.Id,
                Name = tag.Name
            });
        }

        /// <summary>
        /// タグを新たに作成する (Post)
        /// </summary>
        /// <param name="model">タグ</param>
        /// <returns>ビューリザルト</returns>
        [HttpPost("create/{id}/metadata")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateMetadata(int id, [Bind("Id,MetaKey,MetaValue")] CreateTagMetaViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                _context.Add(new TagMeta()
                {
                    TagId = model.Id,
                    MetaKey = model.MetaKey,
                    MetaValue = model.MetaValue
                });
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Details), new { id });
            }
            return View(model);
        }

        /// <summary>
        /// タグのメタデータを編集する
        /// </summary>
        /// <param name="id">タグID</param>
        /// <param name="metaId">タグID</param>
        /// <returns>ビューリザルト</returns>
        [HttpGet("edit/{id}/metadata/{metaId}/metavalue")]
        public async Task<IActionResult> EditMetadataMetaValue(int? id, int? metaId)
        {
            if (id == null || metaId == null)
            {
                return NotFound();
            }

            var tag = await _context.Tags.SingleOrDefaultAsync(m => m.Id == id);
            var tagMeta = await _context.TagMeta.SingleOrDefaultAsync(m => m.TagId == id && m.Id == metaId);
            if (tag == null || tagMeta == null)
            {
                return NotFound();
            }
            return View(new EditTagMetaMetaValueViewModel()
            {
                Id = tag.Id,
                Name = tag.Name,
                MetaId = tagMeta.Id,
                MetaKey = tagMeta.MetaKey,
                MetaValue = tagMeta.MetaValue
            });
        }

        /// <summary>
        /// タグのメタデータを編集する (Post)
        /// </summary>
        /// <param name="id">タグID</param>
        /// <param name="tag">タグ</param>
        /// <returns>ビューリザルト</returns>
        [HttpPost("edit/{id}/metadata/{metaId}/metavalue")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditMetadataMetaValue(int? id, int? metaId, [Bind("Id,MetaId,EditedMetaValue")] EditTagMetaMetaValueViewModel model)
        {
            if (id != model.Id || metaId != model.MetaId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var tagMeta = await _context.TagMeta.FirstOrDefaultAsync(m => m.TagId == id && m.Id == metaId);
                    tagMeta.MetaValue = model.EditedMetaValue;
                    _context.Update(tagMeta);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TagMetaExists(model.Id, model.MetaId))
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
            return View(model);
        }

        /// <summary>
        /// タグのメタデータを削除する
        /// </summary>
        /// <param name="id">タグID</param>
        /// <param name="metaId">タグID</param>
        /// <returns>ビューリザルト</returns>
        [HttpGet("delete/{id}/metadata/{metaId}")]
        public async Task<IActionResult> DeleteMetadata(int? id, int? metaId)
        {
            if (id == null || metaId == null)
            {
                return NotFound();
            }

            var tag = await _context.Tags.SingleOrDefaultAsync(m => m.Id == id);
            var tagMeta = await _context.TagMeta.SingleOrDefaultAsync(m => m.TagId == id && m.Id == metaId);
            if (tag == null || tagMeta == null)
            {
                return NotFound();
            }
            return View(new DeleteTagMetaViewModel()
            {
                Id = tag.Id,
                Name = tag.Name,
                MetaId = tagMeta.Id,
                MetaKey = tagMeta.MetaKey,
                MetaValue = tagMeta.MetaValue
            });
        }

        /// <summary>
        /// タグを削除する (Post)
        /// </summary>
        /// <param name="id">タグID</param>
        /// <returns>ビューリザルト</returns>
        [HttpPost("delete/{id}/metadata/{metaId}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMetadataConfirmed(int id, int metaId)
        {
            var tag = await _context.TagMeta.SingleOrDefaultAsync(m => m.TagId == id && m.Id == metaId);
            _context.TagMeta.Remove(tag);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// タグを削除する
        /// </summary>
        /// <param name="id">タグID</param>
        /// <returns>ビューリザルト</returns>
        [HttpGet("delete/{id}")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tag = await _context.Tags
                .SingleOrDefaultAsync(m => m.Id == id);
            if (tag == null)
            {
                return NotFound();
            }

            return View(tag);
        }

        /// <summary>
        /// タグを削除する (Post)
        /// </summary>
        /// <param name="id">タグID</param>
        /// <returns>ビューリザルト</returns>
        [HttpPost("delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _service.Remove(id);
            return RedirectToAction(nameof(Index));
        }

        private bool TagExists(int id)
        {
            return _context.Tags.Any(e => e.Id == id);
        }

        private bool TagMetaExists(int id, int metaId)
        {
            return _context.TagMeta.Any(e => e.TagId == id && e.Id == metaId);
        }
    }
}
