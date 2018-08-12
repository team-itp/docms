using Docms.Domain.Documents;
using Docms.Infrastructure.Files;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Docms.Web.Application.Commands
{
    public class DeleteDocumentCommandHandler : IRequestHandler<DeleteDocumentCommand, bool>
    {
        private readonly IDocumentRepository _documentRepository;
        private readonly IFileStorage _fileStorage;

        public DeleteDocumentCommandHandler(
            IDocumentRepository documentRepository,
            IFileStorage fileStorage)
        {
            _documentRepository = documentRepository;
            _fileStorage = fileStorage;
        }

        public async Task<bool> Handle(DeleteDocumentCommand request, CancellationToken cancellationToken)
        {
            var document = await _documentRepository.GetAsync(request.Path.ToString());
            if (document == null || document.Deleted != null)
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
