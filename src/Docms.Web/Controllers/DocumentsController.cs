using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Docms.Web.Data;
using Docms.Web.Models;
using Docms.Web.Services;
using Microsoft.WindowsAzure.Storage;
using Microsoft.Azure;
using Microsoft.Extensions.Options;
using Docms.Web.Config;

namespace Docms.Web.Controllers
{
    /// <summary>
    /// ドキュメント情報コントローラー
    /// </summary>
    public class DocumentsController : Controller
    {
        private readonly DocmsDbContext _context;
        private readonly StorageSettings _storageSettings;

        public DocumentsController(DocmsDbContext context, IOptions<StorageSettings> storageSettings)
        {
            _context = context;
            _storageSettings = storageSettings?.Value;
        }

        /// <summary>
        /// ドキュメント情報の一覧を取得する
        /// </summary>
        /// <returns>ドキュメント情報の一覧</returns>
        public async Task<IActionResult> Index()
        {
            return View(await _context.Documents.ToListAsync());
        }

        /// <summary>
        /// 指定したドキュメント情報を取得する
        /// </summary>
        /// <param name="id">ドキュメントID</param>
        /// <returns>ドキュメント情報</returns>
        public async Task<IActionResult> Details(int? id)
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
        /// ドキュメント情報を作成する
        /// </summary>
        public IActionResult Create()
        {
            ViewData["Tags"] = _context.Tags.Select(t => new SelectListItem() { Text = t.Name, Value = t.Name }).ToList();

            return View();
        }

        /// <summary>
        /// ドキュメント情報を作成する
        /// </summary>
        /// <param name="document">ドキュメント情報</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Url,Name,Tags")] UploadDocumentViewModel document)
        {
            if (ModelState.IsValid)
            {
                var account = CloudStorageAccount.Parse(_storageSettings?.ConnectionString);
                var client = account.CreateCloudBlobClient();
                var container = client.GetContainerReference("files");
                await container.CreateIfNotExistsAsync();
                var blob = container.GetBlockBlobReference(Guid.NewGuid().ToString());
                await blob.UploadFromStreamAsync(document.File.OpenReadStream());
                var service = new DocumentsService(_context);
                await service.CreateAsync(blob.Uri.ToString(), document.Name);
                if (document.Tags != null && document.Tags.Length > 0)
                {
                    await service.AddTagsAsync(blob.Uri.ToString(), document.Tags);
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
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var document = await _context.Documents.SingleOrDefaultAsync(m => m.Id == id);
            if (document == null)
            {
                return NotFound();
            }
            return View(document);
        }

        /// <summary>
        /// ドキュメント情報を更新する
        /// </summary>
        /// <param name="id">ドキュメントID</param>
        /// <param name="document">ドキュメント情報</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Url")] Document document)
        {
            if (id != document.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(document);
                    await _context.SaveChangesAsync();
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
                return RedirectToAction(nameof(Index));
            }
            return View(document);
        }

        /// <summary>
        /// ドキュメント情報を削除する
        /// </summary>
        /// <param name="id">ドキュメントID</param>
        /// <returns></returns>
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
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var document = await _context.Documents.SingleOrDefaultAsync(m => m.Id == id);
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
