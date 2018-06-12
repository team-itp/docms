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

        public async Task CreateAsync(string path, Stream fileData, Stream thumbnailData, Tag[] tags, UserInfo user)
        {
            var docInfo = await _storage.SaveAsync(path, fileData);
            if (thumbnailData != null)
            {
                var thumbnailPath = Path.Combine(
                    "thumbnails",
                    Path.GetDirectoryName(path),
                    Path.GetFileNameWithoutExtension(path) + ".png");
                var thumbInfo = await _storage.SaveAsync(path, thumbnailData);
            }
            foreach (var tag in tags)
            {
                await _tags.CreateIfNotExistsAsync(tag);
            }

            var document = new Document()
            {
                Name = docInfo.Name,
                Path = docInfo.Path,
                Size = docInfo.Size,
                MediaType = docInfo.MediaType,
                CreatedAt = docInfo.CreatedAt,
                CreatedBy = user.Id,
                ModifiedAt = docInfo.ModifiedAt,
                ModifiedBy = user.Id,
                Tags = tags,
            };

            await _docs.CreateAsync(document);
        }
    }
}
