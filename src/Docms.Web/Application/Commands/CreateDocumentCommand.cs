using MediatR;
using System.IO;

namespace Docms.Web.Application.Commands
{
    public class CreateDocumentCommand : IRequest<bool>
    {
        public string Path { get; set; }
        public Stream Stream { get; set; }
    }
}
