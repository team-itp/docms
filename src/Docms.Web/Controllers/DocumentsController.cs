﻿using Docms.Web.Data;
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
            ViewData["Tags"] = _context.Tags.ToList();

            return View();
        }

        /// <summary>
        /// ドキュメント情報を作成する
        /// </summary>
        /// <param name="document">ドキュメント情報</param>
        /// <returns></returns>
        [HttpPost]
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
                    await service.CreateAsync(blobName, filename);
                    if (document.Tags != null && document.Tags.Length > 0)
                    {
                        await service.AddTagsAsync(blobName, document.Tags);
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
