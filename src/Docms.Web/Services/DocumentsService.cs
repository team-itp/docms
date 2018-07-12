using Docms.Web.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Docms.Web.Services
{
    public class DocumentsService
    {
        private DocmsDbContext _db;
        private TagsService _tags;

        public DocumentsService(DocmsDbContext db, TagsService tags)
        {
            _db = db;
            _tags = tags;
        }

        public async Task<int> CreateAsync(string blobName, string fileName, string userAccountName)
        {
            var document = new Document()
            {
                BlobName = blobName,
                FileName = fileName,
                UploadedAt = DateTime.Now,
                UploadedBy = userAccountName,
            };
            var entity = _db.Documents.Add(document);
            await _db.SaveChangesAsync().ConfigureAwait(false);
            return entity.Entity.Id;
        }

        public async Task<int> CreateAsync(string blobName, string fileName, string userAccountName, IEnumerable<string> tags)
        {
            var documentId = await CreateAsync(blobName, fileName, userAccountName);
            await AddTagsAsync(documentId, tags, userAccountName);
            return documentId;
        }

        public async Task<int> CreateAsync(string blobName, string fileName, string userAccountName, IEnumerable<string> tags, string personInCharge, string customer, string project)
        {
            var documentId = await CreateAsync(blobName, fileName, userAccountName);
            if (!string.IsNullOrEmpty(personInCharge))
            {
                await AddTagWithCategoryAsync(documentId, personInCharge, Constants.TAG_CATEGORY_PERSON_IN_CHARGE, userAccountName);
            }
            if (!string.IsNullOrEmpty(customer))
            {
                await AddTagWithCategoryAsync(documentId, customer, Constants.TAG_CATEGORY_CUSTOMER, userAccountName);
            }
            if (!string.IsNullOrEmpty(project))
            {
                await AddTagWithCategoryAsync(documentId, project, Constants.TAG_CATEGORY_PROJECT, userAccountName);
            }
            await AddTagsAsync(documentId, tags, userAccountName);
            return documentId;
        }

        public async Task UpdateFileNameAsync(int doucmentId, string editedFileName, string userAccountName)
        {
            var data = await _db.Documents.FindAsync(doucmentId).ConfigureAwait(false);
            data.FileName = editedFileName;
            _db.Update(data);
            await _db.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task AddTagWithCategoryAsync(int doucmentId, string tagname, string category, string userAccountName)
        {
            var document = await _db.Documents
                .Include(e => e.Metadata)
                .Include(e => e.Tags).ThenInclude(e => e.Tag)
                .FirstOrDefaultAsync(e => e.Id == doucmentId);

            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }
            if (string.IsNullOrEmpty(tagname))
            {
                throw new ArgumentNullException(nameof(tagname));
            }

            var tag = await _tags.FindOrCreateAsync(tagname, category);
            _db.Update(tag);
            await _db.SaveChangesAsync();

            document.AddTag(tag);
            await _db.SaveChangesAsync();
        }

        public async Task AddTagsAsync(int doucmentId, IEnumerable<string> tags, string userAccountName)
        {
            var doc = _db.Documents
                .Include(e => e.Tags)
                .ThenInclude(e => e.Tag)
                .FirstOrDefault(d => d.Id == doucmentId);

            if (doc == null)
            {
                // TODO: message
                throw new ArgumentException();
            }

            var tagList = new List<Tag>();
            foreach (var tagname in tags)
            {
                tagList.Add(await _tags.FindOrCreateAsync(tagname));
            }
            _db.UpdateRange(tagList);
            await _db.SaveChangesAsync();

            foreach (var tag in tagList)
            {
                doc.AddTag(tag);
            }
            await _db.SaveChangesAsync();
        }

        public async Task RemoveTagsAsync(int doucmentId, IEnumerable<int> tagIds, string userAccountName)
        {
            var doc = _db.Documents
                 .Include(e => e.Tags)
                 .ThenInclude(e => e.Tag)
                 .FirstOrDefault(d => d.Id == doucmentId);

            if (doc == null)
            {
                // TODO: message
                throw new ArgumentException();
            }

            var tagsToRemove = doc.Tags.Where(t => tagIds.Contains(t.TagId)).ToList();
            foreach (var t in tagsToRemove)
            {
                doc.Tags.Remove(t);
            }
            await _db.SaveChangesAsync();
        }

        public async Task RemoveAsync(int id)
        {
            var document = await _db.Documents
                .Include(e => e.Metadata)
                .Include(e => e.Tags)
                .FirstOrDefaultAsync(e => e.Id == id);
            _db.Documents.Remove(document);
            _db.DocumentMeta.RemoveRange(document.Metadata);
            _db.DocumentTags.RemoveRange(document.Tags);
            await _db.SaveChangesAsync();
        }
    }
}