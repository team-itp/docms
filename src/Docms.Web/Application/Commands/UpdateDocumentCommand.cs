using Docms.Infrastructure.Files;
using MediatR;
using System;
using System.IO;

namespace Docms.Web.Application.Commands
{
    public class UpdateDocumentCommand : IRequest<bool>
    {
        public FilePath Path { get; set; }
        public Stream Stream { get; set; }
        public DateTime? Created { get; set; }
        public DateTime? LastModified { get; set; }
    }
}
