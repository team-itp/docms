using Docms.Domain.Documents;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Docms.Web.Application.Commands
{
    public class DeleteDocumentCommandHandler : IRequestHandler<DeleteDocumentCommand, bool>
    {
        private readonly IDocumentRepository _documentRepository;

        public DeleteDocumentCommandHandler(
            IDocumentRepository documentRepository)
        {
            _documentRepository = documentRepository;
        }

        public async Task<bool> Handle(DeleteDocumentCommand request, CancellationToken cancellationToken = default(CancellationToken))
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
    }
}
