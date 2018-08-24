using Docms.Infrastructure.Files;
using MediatR;
using System;
using System.IO;

namespace Docms.Web.Application.Commands
{
    public class CreateOrUpdateDocumentCommand : IRequest<bool>
    {
        public FilePath Path { get; set; }
        public Stream Stream { get; set; }
        public bool ForceCreate { get; set; } = false;
        public DateTime? Created { get; set; }
        public DateTime? LastModified { get; set; }
    }
}
