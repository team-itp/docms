using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Docms.Web.Docs.Mocks
{
    sealed class TagEqualityComparer : IEqualityComparer<Tag>
    {
        public bool Equals(Tag x, Tag y)
        {
            return x == y || x.Title == y.Title;
        }

        public int GetHashCode(Tag obj)
        {
            return obj?.Title.GetHashCode() ?? -1;
        }
    }

    class InMemoryTagsRepository : ITagsRepository
    {
        private HashSet<Tag> _tags = new HashSet<Tag>(new TagEqualityComparer());
        private static int _maxId = 0;

        public Task CreateIfNotExistsAsync(Tag tag)
        {
            if (_tags.TryGetValue(tag, out var storedTag))
            {
                tag.Id = storedTag.Id;
            }
            else
            {
                tag.Id = ++_maxId;
            }
            return Task.CompletedTask;
        }

        public Task<IEnumerable<Tag>> GetAllAsync()
        {
            return Task.FromResult(_tags.AsEnumerable());
        }
    }
}
