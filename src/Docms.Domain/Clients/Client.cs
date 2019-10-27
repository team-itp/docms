using Docms.Domain.Clients.Events;
using Docms.Domain.SeedWork;
using System;

namespace Docms.Domain.Clients
{
    public class Client : Entity, IAggregateRoot
    {
        public string ClientId { get; set; }
        public string Type { get; set; }
        public string IpAddress { get; set; }
        public string RequestId { get; set; }
        public ClientRequestType RequestType { get; set; }
        public bool IsAccepted { get; set; }
        public DateTime? LastAccessedTime { get; set; }
        public ClientStatus Status { get; set; }

        protected Client()
        {
        }

        public Client(string clientId, string type, string ipAddress)
        {
            ClientId = clientId;
            Type = type ?? throw new ArgumentNullException(nameof(type));
            IpAddress = ipAddress ?? throw new ArgumentNullException(nameof(ipAddress));
            Status = ClientStatus.Stopped;
            OnClientCreated(clientId, type, ipAddress);
        }

        public void Request(ClientRequestType requestType)
        {
            if (!object.Equals(RequestType, requestType))
            {
                var requestId = Guid.NewGuid().ToString();

                RequestId = requestId;
                RequestType = requestType;
                IsAccepted = false;

                OnRequested(requestId, requestType);
            }
        }

        public void Accept(string requestId)
        {
            if (requestId == RequestId && !IsAccepted)
            {
                IsAccepted = true;
                OnRequestAccepted(requestId);
            }
            else
            {
                throw new InvalidOperationException("invalid request id is specified.");
            }
        }

        public void UpdateLastAccessTime()
        {
            var lastAccessedTime = DateTime.UtcNow;
            LastAccessedTime = lastAccessedTime;
            OnLastAccessedTimeUpadated(lastAccessedTime);
        }

        public void UpdateStatus(ClientStatus status)
        {
            Status = status;
            OnStatusUpdated(status);
        }

        private void OnClientCreated(string clientId, string type, string ipAddress)
        {
            var ev = new ClientCreatedEvent(this, clientId, type, ipAddress);
            AddDomainEvent(ev);
        }

        private void OnRequested(string requestId, ClientRequestType requestType)
        {
            var ev = new ClientRequestCreatedEvent(this, ClientId, requestId, requestType);
            AddDomainEvent(ev);
        }

        private void OnRequestAccepted(string requestId)
        {
            var ev = new ClientRequestAcceptedEvent(this, ClientId, requestId, RequestType);
            AddDomainEvent(ev);
        }

        private void OnLastAccessedTimeUpadated(DateTime lastAccessedTime)
        {
            var ev = new ClientLastAccessedTimeUpdatedEvent(this, ClientId, lastAccessedTime);
            AddDomainEvent(ev);
        }

        private void OnStatusUpdated(ClientStatus status)
        {
            var ev = new ClientStatusUpdatedEvent(this, ClientId, status);
            AddDomainEvent(ev);
        }
    }
}
