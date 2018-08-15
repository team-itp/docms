using Docms.Domain.Documents;
using Docms.Infrastructure.Files;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Docms.Web.Application.Commands
{
    public class MoveDocumentCommandHandler : IRequestHandler<MoveDocumentCommand, bool>
    {
        private readonly IDocumentRepository _documentRepository;
        private readonly IFileStorage _fileStorage;

        public MoveDocumentCommandHandler(
            IDocumentRepository documentRepository,
            IFileStorage fileStorage)
        {
            _documentRepository = documentRepository;
            _fileStorage = fileStorage;
        }

        public async Task<bool> Handle(MoveDocumentCommand request, CancellationToken cancellationToken = default(CancellationToken))
        {
            var document = await _documentRepository.GetAsync(request.OriginalPath.ToString());
            if (document == null)
            {
                throw new InvalidOperationException();
            }

            await _fileStorage.MoveAsync(request.OriginalPath, request.DestinationPath);
            document.MoveTo(new DocumentPath(request.DestinationPath.ToString()));
            await _documentRepository.UpdateAsync(document);
            await _documentRepository.UnitOfWork.SaveEntitiesAsync();
            return true;
        }
    }
}
