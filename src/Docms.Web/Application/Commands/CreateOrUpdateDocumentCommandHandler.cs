using Docms.Domain.Documents;
using Docms.Infrastructure.DataStores;
using Docms.Infrastructure.Files;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Docms.Web.Application.Commands
{
    public class CreateOrUpdateDocumentCommandHandler : IRequestHandler<CreateOrUpdateDocumentCommand, bool>
    {
        private readonly IDocumentRepository _documentRepository;
        private readonly IFileStorage _fileStorage;
        private readonly ITemporaryStore _temporaryStore;

        public CreateOrUpdateDocumentCommandHandler(
            IDocumentRepository documentRepository,
            IFileStorage fileStorage,
            ITemporaryStore temporaryStore)
        {
            _documentRepository = documentRepository;
            _fileStorage = fileStorage;
            _temporaryStore = temporaryStore;
        }

        public async Task<bool> Handle(CreateOrUpdateDocumentCommand request, CancellationToken cancellationToken = default(CancellationToken))
        {
            var path = request.Path;
            var entry = await _fileStorage.GetEntryAsync(request.Path);
            if (entry is Directory)
            {
                throw new InvalidOperationException();
            }

            var tempStoreId = Guid.NewGuid();
            try
            {
                // ファイル情報の取得
                await _temporaryStore.SaveAsync(tempStoreId, request.Stream);
                var hash = Hash.CalculateHash(await _temporaryStore.OpenStreamAsync(tempStoreId));
                var fileSize = await _temporaryStore.GetFileSizeAsync(tempStoreId);
                ContentTypeProvider.TryGetContentType(request.Path.Extension, out var contentType);
                if (contentType == null) contentType = "application/octet-stream";

                if (!(entry is File file))
                {
                    var dir = await _fileStorage.GetDirectoryAsync(path.DirectoryPath);
                    var utcNow = DateTime.UtcNow;
                    await _fileStorage.SaveAsync(dir, path.FileName, contentType, await _temporaryStore.OpenStreamAsync(tempStoreId));
                    var document = new Document(new DocumentPath(request.Path.ToString()), contentType, fileSize, hash, request.Created ?? utcNow, request.LastModified ?? request.Created ?? utcNow);
                    await _documentRepository.AddAsync(document);
                }
                else if (request.ForceCreate)
                {
                    var retryCount = 0;
                    var dirPath = request.Path.DirectoryPath;
                    var fnwoe = request.Path.FileNameWithoutExtension;
                    var ext = request.Path.Extension;

                    while (file != null)
                    {
                        retryCount++;
                        entry = await _fileStorage.GetEntryAsync(dirPath.Combine(fnwoe + $"({retryCount})" + ext));
                        if (entry is Directory)
                        {
                            continue;
                        }
                        file = entry as File;
                    }

                    var filename = fnwoe + $"({retryCount})" + ext;
                    var dir = await _fileStorage.GetDirectoryAsync(path.DirectoryPath);
                    var utcNow = DateTime.UtcNow;
                    await _fileStorage.SaveAsync(dir, filename, contentType, await _temporaryStore.OpenStreamAsync(tempStoreId));
                    var document = new Document(new DocumentPath(dirPath.Combine(filename).ToString()), contentType, fileSize, hash, request.Created ?? utcNow, request.LastModified ?? request.Created ?? utcNow);
                    await _documentRepository.AddAsync(document);
                }
                else
                {
                    var document = await _documentRepository.GetAsync(request.Path.ToString());
                    if (hash == document.Hash)
                    {
                        return true;
                    }

                    var utcNow = DateTime.UtcNow;
                    var dir = file.Parent;
                    await _fileStorage.SaveAsync(dir, path.FileName, contentType, await _temporaryStore.OpenStreamAsync(tempStoreId));
                    document.Update(contentType, fileSize, hash, request.Created ?? document.Created, request.LastModified ?? utcNow);
                }
                await _documentRepository.UnitOfWork.SaveEntitiesAsync();
                return true;
            }
            finally
            {
                await _temporaryStore.DeleteAsync(tempStoreId);
            }
        }
    }
}
