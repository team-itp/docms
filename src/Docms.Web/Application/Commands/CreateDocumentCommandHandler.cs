﻿using Docms.Domain.Documents;
using Docms.Infrastructure.Files;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Docms.Web.Application.Commands
{
    public class CreateDocumentCommandHandler : IRequestHandler<CreateDocumentCommand, bool>
    {
        private readonly IDocumentRepository _documentRepository;
        private readonly IFileStorage _fileStorage;

        public CreateDocumentCommandHandler(
            IDocumentRepository documentRepository,
            IFileStorage fileStorage)
        {
            _documentRepository = documentRepository;
            _fileStorage = fileStorage;
        }

        public async Task<bool> Handle(CreateDocumentCommand request, CancellationToken cancellationToken = default(CancellationToken))
        {
            var path = new FilePath(request.Path);
            var dir = await _fileStorage.GetDirectoryAsync(path.DirectoryPath);
            var fileProps = await _fileStorage.SaveAsync(dir, path.FileName, request.Stream);
            var document = new Document(new DocumentPath(request.Path), fileProps.ContentType, fileProps.Size, fileProps.Hash);
            await _documentRepository.AddAsync(document);

            await _documentRepository.UnitOfWork.SaveEntitiesAsync();
            return true;
        }
    }
}
