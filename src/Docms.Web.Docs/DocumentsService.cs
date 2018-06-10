using System.IO;
using System.Threading.Tasks;

namespace Docms.Web.Docs
{
    public class DocumentsService
    {
        private IFileStorage _storage;
        private IDocumentRepository _repository;

        public DocumentsService(IFileStorage storage, IDocumentRepository repository)
        {
            _storage = storage;
            _repository = repository;
        }
        public async Task Create(string path, Stream fileData, UserInfo user)
        {
            var docFileInfo = await _storage.SaveAsync(path, fileData);
            var document = await _repository.Create(docFileInfo, user);
        }
    }
}
