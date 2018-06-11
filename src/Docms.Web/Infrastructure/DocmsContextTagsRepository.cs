using Docms.Web.Docs;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Docms.Web.Infrastructure
{
    public class DocmsContextTagsRepository : ITagsRepository
    {
        private DocmsContext _context;

        public DocmsContextTagsRepository(DocmsContext context)
        {
            _context = context;
        }

        public async Task CreateIfNotExistsAsync(Tag tag)
        {
            await Task.Yield();
            var storedTag = _context.Tags.Where(t => t.Title == tag.Title);
            if (storedTag.Any())
            {
                tag.Id = storedTag.First().Id;
            }
            else
            {
                tag.Id = _context.Tags.Max(v => v.Id) + 1;
                await _context.Tags.AddAsync(new Models.Tag()
                {
                    Id = tag.Id,
                    Title = tag.Title
                });
            }
        }

        public async Task<IEnumerable<Tag>> GetAllAsync()
        {
            await Task.Yield();
            return _context.Tags
                .Select(t => new Tag()
                {
                    Id = t.Id,
                    Title = t.Title
                })
                .ToList().AsEnumerable();
        }
    }
}
