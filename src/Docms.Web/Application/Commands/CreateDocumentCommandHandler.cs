using Docms.Domain.Documents;
using Docms.Infrastructure.Files;
using MediatR;
using System;
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
            var path = request.Path;
            var dir = await _fileStorage.GetDirectoryAsync(path.DirectoryPath);
            var utcNow = DateTime.UtcNow;
            var fileProps = await _fileStorage.SaveAsync(dir, path.FileName, request.Stream, request.Created ?? utcNow, request.LastModified ?? utcNow);
            var document = new Document(new DocumentPath(request.Path.ToString()), fileProps.ContentType, fileProps.Size, fileProps.Hash, fileProps.Created, fileProps.LastModified);
            await _documentRepository.AddAsync(document);

            await _documentRepository.UnitOfWork.SaveEntitiesAsync();
            return true;
        }
    }
}
