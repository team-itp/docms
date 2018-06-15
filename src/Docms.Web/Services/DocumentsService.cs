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

        public async Task CreateAsync(string fileUrl)
        {
            _db.Documents.Add(new Document()
            {
                Url = fileUrl
            });
            await _db.SaveChangesAsync();
        }

        public async Task CreateAsync(string fileUrl, IEnumerable<string> tags)
        {
            await CreateAsync(fileUrl);
            await AddTagsAsync(fileUrl, tags);
        }

        public async Task AddTagsAsync(string fileUrl, IEnumerable<string> tags)
        {
            var doc = _db.Documents
                .Include(e => e.Tags)
                .ThenInclude(e => e.Tag)
                .Where(d => d.Url == fileUrl)
                .FirstOrDefault();

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

        public async Task RemoveTagsAsync(string fileUrl, IEnumerable<string> tags)
        {
            var doc = _db.Documents
                 .Include(e => e.Tags)
                 .ThenInclude(e => e.Tag)
                 .Where(d => d.Url == fileUrl)
                 .FirstOrDefault();

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
    }
}