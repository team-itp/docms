using Docms.Domain.SeedWork;
using System;

namespace Docms.Domain.Clients.Events
{
    public class ClientCreatedEvent : DomainEvent<Client>
    {
        public string ClientId { get; }
        public string Type { get; }
        public string IpAddress { get; }

        public ClientCreatedEvent(Client client, string clientId, string type, string ipAddress) : base(client)
        {
            ClientId = clientId;
            Type = type;
            IpAddress = ipAddress;
        }
    }
}