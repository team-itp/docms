using MediatR;

namespace Docms.Application.Commands
{
    public class UpdateSecurityStampCommand : IRequest<bool>
    {
        public string UserId { get; set; }
        public string SecurityStamp { get; set; }
    }
}
