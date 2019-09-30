using Docms.Domain.Documents;
using Docms.Infrastructure.Files;
using MediatR;
using System;

namespace Docms.Web.Application.Commands
{
    public class CreateOrUpdateDocumentCommand : IRequest<bool>
    {
        public FilePath Path { get; set; }
        public IData Data { get; set; }
        public bool ForceCreate { get; set; } = false;
        public DateTime? Created { get; set; }
        public DateTime? LastModified { get; set; }
    }
}
