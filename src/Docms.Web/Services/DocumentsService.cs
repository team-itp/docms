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

        public DocumentsService(DocmsDbContext db)
        {
            _db = db;
        }

        public async Task<int> CreateAsync(string blobName, string name)
        {
            var entity = _db.Documents.Add(new Document()
            {
                BlobName = blobName,
                FileName = name,
                UploadedAt = DateTime.Now,
            });
            await _db.SaveChangesAsync();
            return entity.Entity.Id;
        }

        public async Task<int> CreateAsync(string blobName, string name, IEnumerable<string> tags)
        {
            var documentId = await CreateAsync(blobName, name);
            await AddTagsAsync(documentId, tags);
            return documentId;
        }

        public async Task AddTagsAsync(int doucmentId, IEnumerable<string> tags)
        {
            var doc = _db.Documents
                .Include(e => e.Tags)
                .ThenInclude(e => e.Tag)
                .FirstOrDefault(d => d.Id == doucmentId);

            await AddTagsAsync(doc, tags);
        }

        public async Task AddTagsAsync(string blobName, IEnumerable<string> tags)
        {
            var doc = _db.Documents
                .Include(e => e.Tags)
                .ThenInclude(e => e.Tag)
                .FirstOrDefault(d => d.BlobName == blobName);

            await AddTagsAsync(doc, tags);
        }

        private async Task AddTagsAsync(Document doc, IEnumerable<string> tags)
        {
            if (doc == null)
            {
                // TODO: message
                throw new ArgumentException();
            }

            var dbTags = _db.Tags.Where(t => tags.Contains(t.Name)).ToList();
            var tagsToInsert = tags.Except(dbTags.Select(t => t.Name)).Select(t => new Tag()
            {
                Name = t
            }).ToList();
            await _db.Tags.AddRangeAsync(tagsToInsert);
            await _db.SaveChangesAsync();
            dbTags.AddRange(tagsToInsert);

            foreach (var tag in dbTags)
            {
                doc.AddTag(tag);
            }
            await _db.SaveChangesAsync();
        }

        public async Task AddTagsByIdAsync(int doucmentId, IEnumerable<int> tagIds)
        {
            var doc = _db.Documents
                .Include(e => e.Tags)
                .ThenInclude(e => e.Tag)
                .FirstOrDefault(d => d.Id == doucmentId);

            var dbTags = _db.Tags.Where(t => tagIds.Contains(t.Id)).ToList();
            var tagsNotInDb = tagIds.Except(dbTags.Select(t => t.Id));
            if (tagsNotInDb.Any())
            {
                //TODO: message
                throw new InvalidOperationException();
            }

            foreach (var tag in dbTags)
            {
                doc.AddTag(tag);
            }
            await _db.SaveChangesAsync();
        }

        public async Task RemoveTagsAsync(int documentId, IEnumerable<string> tags)
        {
            var doc = _db.Documents
                 .Include(e => e.Tags)
                 .ThenInclude(e => e.Tag)
                 .FirstOrDefault(d => d.Id == documentId);

            if (doc == null)
            {
                // TODO: message
                throw new ArgumentException();
            }

            var tagsToRemove = doc.Tags.Where(t => tags.Contains(t.Tag.Name)).ToList();
            foreach(var t in tagsToRemove)
            {
                doc.Tags.Remove(t);
            }
            await _db.SaveChangesAsync();
        }

        public async Task RemoveTagsByIdAsync(int doucmentId, IEnumerable<int> tagIds)
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
    }
}