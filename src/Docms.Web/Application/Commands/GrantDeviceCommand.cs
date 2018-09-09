﻿using MediatR;

namespace Docms.Web.Application.Commands
{
    public class GrantDeviceCommand : IRequest<bool>
    {
        public string DeviceId { get; set; }
        public string ByUserId { get; set; }
    }
}
