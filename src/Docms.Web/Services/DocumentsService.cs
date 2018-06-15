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

            var tagsToInsert = doc.Tags.Select(t => t.Tag.Name).Except(tags);
            var dbTags = tagsToInsert.Select(t => new Tag()
            {
                Name = t
            }).ToList();
            await _db.Tags.AddRangeAsync(dbTags);
            await _db.SaveChangesAsync();

            foreach (var tag in dbTags)
            {
                doc.AddTag(tag);
            }
            await _db.SaveChangesAsync();
        }
    }
}