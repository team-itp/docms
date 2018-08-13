using Docms.Infrastructure.Files;
using MediatR;

namespace Docms.Web.Application.Commands
{
    public class MoveDocumentCommand : IRequest<bool>
    {
        public FilePath OriginalPath { get; set; }
        public FilePath DestinationPath { get; set; }
    }
}
