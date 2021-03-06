﻿using MediatR;
using System;

namespace Docms.Application.Commands
{
    public class RegisterClientCommand : IRequest<bool>
    {
        public string ClientId { get; set; }
        public string Type { get; set; }
        public string IpAddress { get; set; }
    }
}
