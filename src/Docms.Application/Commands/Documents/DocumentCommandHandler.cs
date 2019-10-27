using Docms.Domain.Documents;
using Docms.Infrastructure.Storage;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Docms.Application.Commands
{
    public class DocumentCommandHandler : 
        IRequestHandler<CreateOrUpdateDocumentCommand, bool>,
        IRequestHandler<DeleteDocumentCommand, bool>,
        IRequestHandler<MoveDocumentCommand, bool>
    {
        private readonly IDocumentRepository _documentRepository;
        private readonly IDataStore _dataStore;

        public DocumentCommandHandler(
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
            try
            {
                if (!ContentTypeProvider.TryGetContentType(path.Extension, out var contentType))
                    contentType = "application/octet-stream";
                if (document == null)
                {
                    var utcNow = DateTime.UtcNow;
                    document = new Document(
                        new DocumentPath(path.ToString()),
                        contentType,
                        request.Data,
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
                        contentType,
                        request.Data,
                        request.Created ?? utcNow,
                        request.LastModified ?? request.Created ?? utcNow);
                    await _documentRepository.AddAsync(document).ConfigureAwait(false);
                }
                else
                {
                    var utcNow = DateTime.UtcNow;

                    if (request.Data.Hash == document.Hash)
                    {
                        await _dataStore.DeleteAsync(request.Data.StorageKey).ConfigureAwait(false);
                        var oldData = await _dataStore.FindAsync(document.StorageKey);
                        if (oldData == null)
                        {
                            document.Update(
                                contentType,
                                request.Data,
                                request.Created ?? document.Created,
                                request.LastModified ?? utcNow);
                        }
                        else
                        {
                            document.Update(
                                contentType,
                                oldData,
                                request.Created ?? document.Created,
                                request.LastModified ?? utcNow);
                        }
                    }
                    else
                    {
                        document.Update(
                            contentType,
                            request.Data,
                            request.Created ?? document.Created,
                            request.LastModified ?? utcNow);
                    }
                    await _documentRepository.UpdateAsync(document);
                }
                await _documentRepository.UnitOfWork.SaveEntitiesAsync().ConfigureAwait(false);
                return true;
            }
            catch
            {
                await _dataStore.DeleteAsync(request.Data.StorageKey).ConfigureAwait(false);
                throw;
            }
        }

        public async Task<bool> Handle(DeleteDocumentCommand request, CancellationToken cancellationToken = default)
        {
            var document = await _documentRepository.GetAsync(request.Path.ToString());
            if (document == null)
            {
                throw new InvalidOperationException();
            }
            document.Delete();
            await _documentRepository.UpdateAsync(document);
            await _documentRepository.UnitOfWork.SaveEntitiesAsync();
            return true;
        }

        public async Task<bool> Handle(MoveDocumentCommand request, CancellationToken cancellationToken = default)
        {
            var document = await _documentRepository.GetAsync(request.OriginalPath.ToString());
            if (document == null)
            {
                throw new InvalidOperationException();
            }

            document.MoveTo(new DocumentPath(request.DestinationPath.ToString()));
            await _documentRepository.UpdateAsync(document);
            await _documentRepository.UnitOfWork.SaveEntitiesAsync();
            return true;
        }
    }
}
