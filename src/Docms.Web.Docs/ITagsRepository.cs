using System.Collections.Generic;
using System.Threading.Tasks;

namespace Docms.Web.Docs
{
    public interface ITagsRepository
    {
        Task CreateIfNotExistsAsync(Tag tag);
        Task<IEnumerable<Tag>> GetAllAsync();
    }
}