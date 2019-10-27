using Docms.Domain.SeedWork;
using System;

namespace Docms.Domain.Clients.Events
{
    public class ClientLastAccessedTimeUpdatedEvent : DomainEvent<Client>
    {
        public string ClientId { get; }
        public DateTime LastAccessedTime { get; }

        public ClientLastAccessedTimeUpdatedEvent(Client client, string clientId, DateTime lastAccessedTime) : base(client)
        {
            ClientId = clientId;
            LastAccessedTime = lastAccessedTime;
        }
    }
}