using System.Threading.Tasks;

namespace Docms.Web.Docs
{
    public interface IDocumentsRepository
    {
        Task CreateAsync(Document document);
        Task<Document> FindAsync(int id);
    }
}
