using Docms.Domain.Documents;
using Docms.Infrastructure.Storage;
using Docms.Queries.Blobs;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Docms.Web.Application.Commands
{
    public class CreateOrUpdateDocumentCommandHandler : IRequestHandler<CreateOrUpdateDocumentCommand, bool>
    {
        private readonly IDocumentRepository _documentRepository;
        private readonly IDataStore _dataStore;

        public CreateOrUpdateDocumentCommandHandler(
            IDocumentRepository documentRepository,
            IDataStore dataStore)
        {
            _documentRepository = documentRepository;
            _dataStore = dataStore;
        }

        public async Task<bool> Handle(CreateOrUpdateDocumentCommand request, CancellationToken cancellationToken = default)
        {
            var path = request.Path;
            if (await _documentRepository.IsContainerPath(path.ToString()))
            {
                throw new InvalidOperationException();
            }

            // ドキュメントの存在チェック
            var document = await _documentRepository.GetAsync(path.ToString());

            // ファイル情報の取得
            var storageKey = _dataStore.CreateKey();
            var data = await _dataStore.CreateAsync(storageKey, request.Stream, request.SizeOfStream).ConfigureAwait(false);

            try
            {
                if (!ContentTypeProvider.TryGetContentType(path.Extension, out var contentType))
                    contentType = "application/octet-stream";
                if (document == null)
                {
                    var utcNow = DateTime.UtcNow;
                    document = new Document(
                        new DocumentPath(path.ToString()),
                        storageKey,
                        contentType,
                        data,
                        request.Created ?? utcNow,
                        request.LastModified ?? request.Created ?? utcNow);
                    await _documentRepository.AddAsync(document).ConfigureAwait(false);
                }
                else if (request.ForceCreate)
                {
                    var retryCount = 0;
                    var dirPath = path.DirectoryPath;
                    var fnwoe = path.FileNameWithoutExtension;
                    var ext = path.Extension;
                    var filename = path;
                    while (document != null)
                    {
                        retryCount++;
                        filename = dirPath.Combine(fnwoe + $"({retryCount})" + ext);
                        document = await _documentRepository.GetAsync(filename.ToString());
                    }

                    var utcNow = DateTime.UtcNow;
                    document = new Document(
                        new DocumentPath(filename.ToString()),
                        storageKey,
                        contentType,
                        data,
                        request.Created ?? utcNow,
                        request.LastModified ?? request.Created ?? utcNow);
                    await _documentRepository.AddAsync(document).ConfigureAwait(false);
                }
                else
                {
                    if (data.Hash == document.Hash)
                    {
                        await _dataStore.DeleteAsync(storageKey).ConfigureAwait(false);
                        return true;
                    }

                    var utcNow = DateTime.UtcNow;
                    document.Update(
                        storageKey,
                        contentType,
                        data,
                        request.Created ?? document.Created,
                        request.LastModified ?? utcNow);
                }
                await _documentRepository.UnitOfWork.SaveEntitiesAsync().ConfigureAwait(false);
                return true;
            }
            catch
            {
                await _dataStore.DeleteAsync(storageKey).ConfigureAwait(false);
                throw;
            }
        }
    }
}
