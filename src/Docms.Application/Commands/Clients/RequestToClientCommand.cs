using Docms.Domain.Clients;
using MediatR;

namespace Docms.Application.Commands
{
    public class RequestToClientCommand : IRequest<bool>
    {
        public string ClientId { get; set; }
        public ClientRequestType RequestType { get; set; }
    }
}
