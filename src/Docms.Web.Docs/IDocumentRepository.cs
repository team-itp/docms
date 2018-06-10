using System.Threading.Tasks;

namespace Docms.Web.Docs
{
    public interface IDocumentRepository
    {
        Task<Document> Create(DocumentFileInfo fileInfo, UserInfo user);
    }
}
