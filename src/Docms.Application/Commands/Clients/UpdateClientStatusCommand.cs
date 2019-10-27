using Docms.Domain.Clients;
using MediatR;

namespace Docms.Application.Commands
{
    public class UpdateClientStatusCommand : IRequest<bool>
    {
        public string ClientId { get; set; }
        public ClientStatus ClientStatus { get; set; }
    }
}