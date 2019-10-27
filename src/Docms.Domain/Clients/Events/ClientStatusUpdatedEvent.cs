using Docms.Domain.SeedWork;
using System;

namespace Docms.Domain.Clients.Events
{
    public class ClientStatusUpdatedEvent : DomainEvent<Client>
    {
        public string ClientId { get; }
        public ClientStatus Status { get; }

        public ClientStatusUpdatedEvent(Client client, string clientId, ClientStatus status) : base(client)
        {
            ClientId = clientId;
            Status = status;
        }
    }
}