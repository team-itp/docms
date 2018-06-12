using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Model = Docms.Web.Docs;

namespace Docms.Web.Infrastructure.Docs
{
    public class DocmsContextTagsRepository : Model.ITagsRepository
    {
        private DocmsContext _context;

        public DocmsContextTagsRepository(DocmsContext context)
        {
            _context = context;
        }

        public async Task CreateIfNotExistsAsync(Model.Tag tag)
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
                await _context.Tags.AddAsync(new Tag()
                {
                    Id = tag.Id,
                    Title = tag.Title
                });
            }
        }

        public async Task<IEnumerable<Model.Tag>> GetAllAsync()
        {
            await Task.Yield();
            return _context.Tags
                .Select(t => new Model.Tag()
                {
                    Id = t.Id,
                    Title = t.Title
                })
                .ToList().AsEnumerable();
        }
    }
}
