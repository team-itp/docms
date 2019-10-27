using Docms.Domain.Documents;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Docms.Application.Commands
{
    public class MoveDocumentCommandHandler : IRequestHandler<MoveDocumentCommand, bool>
    {
        private readonly IDocumentRepository _documentRepository;

        public MoveDocumentCommandHandler(
            IDocumentRepository documentRepository)
        {
            _documentRepository = documentRepository;
        }

        public async Task<bool> Handle(MoveDocumentCommand request, CancellationToken cancellationToken = default(CancellationToken))
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
