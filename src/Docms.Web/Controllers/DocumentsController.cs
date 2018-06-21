using Docms.Web.Data;
using Docms.Web.Models;
using Docms.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Docms.Web.Controllers
{
    /// <summary>
    /// ドキュメント情報コントローラー
    /// </summary>
    [Route("documents")]
    public class DocumentsController : Controller
    {
        private readonly DocmsDbContext _context;
        private readonly IStorageService _storageService;

        public DocumentsController(DocmsDbContext context, IStorageService storageService)
        {
            _context = context;
            _storageService = storageService;
        }

        /// <summary>
        /// ドキュメント情報の一覧を取得する
        /// </summary>
        /// <returns>ドキュメント情報の一覧</returns>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            return View(await _context.Documents.ToListAsync());
        }

        /// <summary>
        /// 指定したドキュメント情報を取得する
        /// </summary>
        /// <param name="id">ドキュメントID</param>
        /// <returns>ドキュメント情報</returns>
        [HttpGet("details/{id}")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var document = await _context.Documents
                .Include(d => d.Tags)
                .ThenInclude(dt => dt.Tag)
                .SingleOrDefaultAsync(m => m.Id == id);
            if (document == null)
            {
                return NotFound();
            }

            return View(DocumentViewModel.Create(Url, document));
        }

        /// <summary>
        /// ドキュメント情報を作成する
        /// </summary>
        [HttpGet("create")]
        public IActionResult Create()
        {
            ViewData["Tags"] = _context.Tags.ToList();

            return View();
        }

        /// <summary>
        /// ドキュメント情報を作成する
        /// </summary>
        /// <param name="document">ドキュメント情報</param>
        /// <returns></returns>
        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Files,Tags")] UploadDocumentViewModel document)
        {
            if (document.Files == null || document.Files.Count == 0)
            {
                ModelState.AddModelError("Files", "ファイルは1件以上指定してください。");
            }

            if (ModelState.IsValid)
            {
                foreach (var file in document.Files)
                {
                    var filename = Path.GetFileName(file.FileName);
                    var blobName = await _storageService.UploadFileAsync(file.OpenReadStream(), Path.GetExtension(file.FileName));
                    var service = new DocumentsService(_context);
                    var documentId = await service.CreateAsync(blobName, filename);
                    if (document.Tags != null && document.Tags.Length > 0)
                    {
                        await service.AddTagsAsync(documentId, document.Tags);
                    }
                }

                return RedirectToAction(nameof(Index));
            }
            return View(document);
        }

        /// <summary>
        /// ドキュメント情報を更新する
        /// </summary>
        /// <param name="id">ドキュメントID</param>
        /// <returns></returns>
        [HttpGet("edit/{id}/filename")]
        public async Task<IActionResult> EditFileName(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var document = await _context.Documents
                .SingleOrDefaultAsync(m => m.Id == id);

            if (document == null)
            {
                return NotFound();
            }

            return View(new EditFileNameViewModel()
            {
                Id = document.Id,
                FileName = document.FileName,
                EditedFileName = document.FileName
            });
        }

        /// <summary>
        /// ドキュメント情報を更新する
        /// </summary>
        /// <param name="id">ドキュメントID</param>
        /// <param name="document">ドキュメント情報</param>
        /// <returns></returns>
        [HttpPost("edit/{id}/filename")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditFileName(int id, [Bind("Id,EditedFileName")] EditFileNameViewModel document)
        {
            if (id != document.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var service = new DocumentsService(_context);
                    await service.UpdateFileNameAsync(document.Id, document.EditedFileName);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DocumentExists(document.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Details), new { id = id });
            }
            return View(document);
        }

        /// <summary>
        /// ドキュメント情報にタグを追加する
        /// </summary>
        /// <param name="id">ドキュメントID</param>
        /// <returns></returns>
        [HttpGet("edit/{id}/tags/add")]
        public async Task<IActionResult> AddTags(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var document = await _context.Documents
                .SingleOrDefaultAsync(m => m.Id == id);

            if (document == null)
            {
                return NotFound();
            }

            ViewData["Tags"] = _context.Tags
                .OrderBy(t => t.Name)
                .Select(t => new SelectListItem() { Text = t.Name, Value = t.Name });

            return View(new AddTagsViewModel()
            {
                Id = document.Id,
                FileName = document.FileName,
            });
        }

        /// <summary>
        /// ドキュメント情報にタグを追加する
        /// </summary>
        /// <param name="id">ドキュメントID</param>
        /// <param name="tags">タグ情報</param>
        /// <returns></returns>
        [HttpPost("edit/{id}/tags/add")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddTags(int id, [Bind("Id,Tags")] AddTagsViewModel document)
        {
            if (id != document.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var service = new DocumentsService(_context);
                    await service.AddTagsAsync(document.Id, document.Tags);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DocumentExists(document.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Details), new { id = id });
            }
            return View(document);
        }

        /// <summary>
        /// ドキュメント情報からタグを削除する
        /// </summary>
        /// <param name="id">ドキュメントID</param>
        /// <param name="tagId">タグ名</param>
        /// <returns></returns>
        [HttpGet("edit/{id}/tags/{tagId}/delete")]
        public async Task<IActionResult> DeleteTag(int? id, int? tagId)
        {
            if (id == null || tagId == null)
            {
                return NotFound();
            }

            var document = await _context.Documents
                .Include(m => m.Tags)
                .ThenInclude(m => m.Tag)
                .SingleOrDefaultAsync(m => m.Id == id);

            if (document == null)
            {
                return NotFound();
            }

            var tag = document.Tags.SingleOrDefault(t => t.TagId == tagId);

            if (tag == null)
            {
                return NotFound();
            }

            return View(new DeleteTagViewModel()
            {
                Id = document.Id,
                TagId = tag.TagId,
                FileName = document.FileName,
                Name = tag.Tag.Name,
            });
        }

        /// <summary>
        /// ドキュメント情報にタグを追加する
        /// </summary>
        /// <param name="id">ドキュメントID</param>
        /// <param name="tags">タグ情報</param>
        /// <returns></returns>
        [HttpPost("edit/{id}/tags/{tagId}/delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTagConfirmed(int id, int tagId)
        {
            var service = new DocumentsService(_context);
            await service.RemoveTagsByIdAsync(id, new[] { tagId });
            return RedirectToAction(nameof(Details), new { id = id });
        }

        /// <summary>
        /// ドキュメント情報を削除する
        /// </summary>
        /// <param name="id">ドキュメントID</param>
        /// <returns></returns>
        [HttpGet("delete/{id}")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var document = await _context.Documents
                .SingleOrDefaultAsync(m => m.Id == id);
            if (document == null)
            {
                return NotFound();
            }

            return View(document);
        }

        /// <summary>
        /// ドキュメント情報を削除する
        /// </summary>
        /// <param name="id">ドキュメントID</param>
        /// <returns></returns>
        [HttpPost("delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var document = await _context.Documents.SingleOrDefaultAsync(m => m.Id == id);
            await _storageService.DeleteFileAsync(document.BlobName);
            _context.Documents.Remove(document);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DocumentExists(int id)
        {
            return _context.Documents.Any(e => e.Id == id);
        }
    }
}
