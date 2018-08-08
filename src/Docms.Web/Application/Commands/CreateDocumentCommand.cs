using MediatR;

namespace Docms.Web.Application.Commands
{
    public class CreateDocumentCommand : IRequest<bool>
    {
        public string Path { get; set; }
        public string ContentType { get; set; }
        public long FileSize { get; set; }
        public byte[] Hash { get; set; }
    }
}
