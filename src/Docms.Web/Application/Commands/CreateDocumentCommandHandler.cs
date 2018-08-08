using Docms.Domain.Documents;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Docms.Web.Application.Commands
{
    public class CreateDocumentCommandHandler : IRequestHandler<CreateDocumentCommand, bool>
    {
        private IDocumentRepository _documentRepository;

        public CreateDocumentCommandHandler(IDocumentRepository documentRepository)
        {
            _documentRepository = documentRepository;
        }

        public async Task<bool> Handle(CreateDocumentCommand request, CancellationToken cancellationToken = default(CancellationToken))
        {
            var document = new Document(new DocumentPath(request.Path), request.ContentType, request.FileSize, request.Hash);
            await _documentRepository.AddAsync(document);

            await _documentRepository.UnitOfWork.SaveEntitiesAsync();
            return true;
        }
    }
}
