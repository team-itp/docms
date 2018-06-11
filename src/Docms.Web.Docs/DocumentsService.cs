using System.IO;
using System.Threading.Tasks;

namespace Docms.Web.Docs
{
    public class DocumentsService
    {
        private IFileStorage _storage;
        private IDocumentsRepository _docs;
        private ITagsRepository _tags;

        public DocumentsService(IFileStorage storage, IDocumentsRepository docs, ITagsRepository tags)
        {
            _storage = storage;
            _docs = docs;
            _tags = tags;
        }
        public async Task CreateAsync(string path, Stream fileData, Tag[] tags, UserInfo user)
        {
            var docInfo = await _storage.SaveAsync(path, fileData);
            foreach (var tag in tags)
            {
                await _tags.CreateIfNotExistsAsync(tag);
            }

            var document = new Document()
            {
                Id = -1,
                Name = docInfo.Name,
                Path = docInfo.Path,
                Size = docInfo.Size,
                MediaType = docInfo.MediaType,
                Created = docInfo.Created,
                Modified = docInfo.Modified,
                Tags = tags,
            };

            await _docs.CreateAsync(document);
        }
    }
}
