using Docms.Domain.SeedWork;
using System;

namespace Docms.Domain.Clients.Events
{
    public class ClientRequestCreatedEvent : DomainEvent<Client>
    {
        public string ClientId { get; }
        public string RequestId { get; }
        public ClientRequestType RequestType { get; }

        public ClientRequestCreatedEvent(Client client, string clientId, string requestId, ClientRequestType requestType) : base(client)
        {
            ClientId = clientId;
            RequestId = requestId;
            RequestType = requestType;
        }
    }
}