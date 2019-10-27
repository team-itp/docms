using MediatR;

namespace Docms.Application.Commands
{
    public class AcceptRequestFromUserCommand : IRequest<bool>
    {
        public string ClientId { get; set; }
        public string RequestId { get; set; }
    }
}
