using Docms.Web.Data;
using Docms.Web.Models;
using Docms.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Docms.Web.Controllers
{
    /// <summary>
    /// ドキュメント情報コントローラー
    /// </summary>
    [Authorize]
    [Route("documents")]
    public class DocumentsController : Controller
    {
        private readonly DocmsDbContext _context;
        private readonly DocumentsService _service;
        private readonly IStorageService _storageService;
        private readonly UserManager<ApplicationUser> _userManager;

        public DocumentsController(
            DocmsDbContext context,
            DocumentsService service,
            IStorageService storageService,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _service = service;
            _storageService = storageService;
            _userManager = userManager;
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
        public async Task<IActionResult> Details(int? id, [FromQuery(Name = "r")] string referer)
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

            ViewData["Referer"] = referer;

            return View(DocumentViewModel.Create(Url, document));
        }

        /// <summary>
        /// ドキュメント情報を作成する
        /// </summary>
        [HttpGet("create")]
        public IActionResult Create()
        {
            ViewData["PersonInChargeList"] = _context.Tags.Include(t => t.Metadata).Where(e => e.Metadata.Any(md => md.MetaKey == Constants.TAG_KEY_CATEGORY && md.MetaValue == Constants.TAG_CATEGORY_PERSON_IN_CHARGE));
            ViewData["CustomerList"] = _context.Tags.Include(t => t.Metadata).Where(e => e.Metadata.Any(md => md.MetaKey == Constants.TAG_KEY_CATEGORY && md.MetaValue == Constants.TAG_CATEGORY_CUSTOMER));
            ViewData["ProjectList"] = _context.Tags.Include(t => t.Metadata).Where(e => e.Metadata.Any(md => md.MetaKey == Constants.TAG_KEY_CATEGORY && md.MetaValue == Constants.TAG_CATEGORY_PROJECT));
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
        public async Task<IActionResult> Create(UploadDocumentViewModel document)
        {
            if (document.Files == null || document.Files.Count == 0)
            {
                ModelState.AddModelError("Files", "ファイルは1件以上指定してください。");
            }

            if (ModelState.IsValid)
            {
                var uploadedUser = User.Identity.Name;
                foreach (var file in document.Files)
                {
                    var filename = Path.GetFileName(file.FileName);
                    new FileExtensionContentTypeProvider().TryGetContentType(file.FileName, out var contentType);
                    var blobName = await _storageService.UploadFileAsync(file.OpenReadStream(), contentType);
                    var documentId = await _service.CreateAsync(blobName, filename, uploadedUser, document.Tags.Where(t => !string.IsNullOrEmpty(t)), document.PersonInCharge, document.Customer, document.Project);
                }

                return RedirectToAction("Index", "Home");
            }
            return View(document);
        }

        /// <summary>
        /// ドキュメント情報を更新する
        /// </summary>
        /// <param name="id">ドキュメントID</param>
        /// <returns></returns>
        [HttpGet("{id}/edit/filename")]
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
        [HttpPost("{id}/edit/filename")]
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
                    var uploadedUser = User.Identity.Name;
                    await _service.UpdateFileNameAsync(document.Id, document.EditedFileName, uploadedUser);
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
                return RedirectToAction(nameof(Details), new { id });
            }
            return View(document);
        }

        /// <summary>
        /// ドキュメント情報にタグを追加する
        /// </summary>
        /// <param name="id">ドキュメントID</param>
        /// <returns></returns>
        [HttpGet("{id}/tags/add")]
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

            ViewData["Tags"] = await _context.Tags
                .OrderBy(t => t.Name)
                .ToListAsync();

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
        [HttpPost("{id}/tags/add")]
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
                    var userAccountName = User.Identity.Name;
                    await _service.AddTagsAsync(document.Id, document.Tags.Where(t => !string.IsNullOrEmpty(t)), userAccountName);
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
                return RedirectToAction(nameof(Details), new { id });
            }
            return View(document);
        }

        /// <summary>
        /// ドキュメント情報からタグを削除する
        /// </summary>
        /// <param name="id">ドキュメントID</param>
        /// <param name="tagId">タグ名</param>
        /// <returns></returns>
        [HttpGet("{id}/tags/delete/{tagId}")]
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
        [HttpPost("{id}/tags/delete/{tagId}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTagConfirmed(int id, int tagId)
        {
            var userAccountName = User.Identity.Name;
            await _service.RemoveTagsAsync(id, new[] { tagId }, userAccountName);
            return RedirectToAction(nameof(Details), new { id });
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
            await _service.RemoveAsync(document.Id);
            return RedirectToAction("Index", "Home");
        }

        private bool DocumentExists(int id)
        {
            return _context.Documents.Any(e => e.Id == id);
        }
    }
}
