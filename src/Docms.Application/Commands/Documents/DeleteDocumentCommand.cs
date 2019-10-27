using Docms.Infrastructure.Files;
using MediatR;

namespace Docms.Application.Commands
{
    public class DeleteDocumentCommand : IRequest<bool>
    {
        public FilePath Path { get; set; }
    }
}
