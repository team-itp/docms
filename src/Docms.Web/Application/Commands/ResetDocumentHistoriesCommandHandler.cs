﻿using Docms.Domain.Documents;
using Docms.Infrastructure;
using Docms.Infrastructure.DataStores;
using Docms.Infrastructure.Files;
using Docms.Web.Application.Queries;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Docms.Web.Application.Commands
{
    public class ResetDocumentHistoriesCommandHandler : IRequestHandler<ResetDocumentHistoriesCommand, bool>
    {
        private readonly DocmsContext _context;
        private readonly DocmsQueriesContext _queriesContext;
        private readonly IDocumentRepository _documentRepository;
        private readonly IFileStorage _fileStorage;
        private readonly ITemporaryStore _temporaryStore;

        public ResetDocumentHistoriesCommandHandler(
            DocmsContext context,
            DocmsQueriesContext queriesContext,
            IDocumentRepository documentRepository,
            IFileStorage fileStorage,
            ITemporaryStore temporaryStore)
        {
            _context = context;
            _queriesContext = queriesContext;
            _documentRepository = documentRepository;
            _fileStorage = fileStorage;
            _temporaryStore = temporaryStore;
        }

        public async Task<bool> Handle(ResetDocumentHistoriesCommand request, CancellationToken cancellationToken = default(CancellationToken))
        {
            await _context.Database.EnsureDeletedAsync();
            await _context.Database.EnsureCreatedAsync();
            await _queriesContext.Database.EnsureDeletedAsync();
            await _queriesContext.Database.EnsureCreatedAsync();

            var dir = await _fileStorage.GetDirectoryAsync("");
            await RecreateAllFilesAsync(dir);

            return true;
        }

        private async Task RecreateAllFilesAsync(Directory dir)
        {
            foreach (var entry in await _fileStorage.GetEntriesAsync(dir))
            {
                if (entry is Directory dirEntry)
                {
                    await RecreateAllFilesAsync(dirEntry);
                }
                if (entry is File fileEntry)
                {
                    await AddFileAsync(fileEntry);
                }
            }
        }

        private async Task AddFileAsync(File fileEntry)
        {
            var tempStoreId = Guid.NewGuid();
            try
            {
                // ファイル情報の取得
                await _temporaryStore.SaveAsync(tempStoreId, await fileEntry.OpenAsync());
                var hash = Hash.CalculateHash(await _temporaryStore.OpenStreamAsync(tempStoreId));
                var fileSize = await _temporaryStore.GetFileSizeAsync(tempStoreId);
                ContentTypeProvider.TryGetContentType(fileEntry.Path.Extension, out var contentType);
                if (contentType == null) contentType = "application/octet-stream";

                var utcNow = DateTime.UtcNow;
                var document = new Document(new DocumentPath(fileEntry.Path.ToString()), contentType, fileSize, hash, utcNow, utcNow);
                await _documentRepository.AddAsync(document);
                await _documentRepository.UnitOfWork.SaveEntitiesAsync();
            }
            finally
            {
                await _temporaryStore.DeleteAsync(tempStoreId);
            }
        }
    }
}

