using MediatR;
using System;

namespace Docms.Application.Commands
{
    public class RegisterClientCommand : IRequest<bool>
    {
        public Guid ClientId { get; set; }
        public string Type { get; set; }
        public string IPAddress { get; set; }
    }
}
