using Docms.Infrastructure.Files;
using MediatR;

namespace Docms.Web.Application.Commands
{
    public class DeleteDocumentCommand : IRequest<bool>
    {
        public FilePath Path { get; set; }
    }
}
