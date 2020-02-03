using Docms.Domain.Identity.Events;
using Docms.Domain.SeedWork;
using System;

namespace Docms.Domain.Identity
{
    public class Device : Entity, IAggregateRoot
    {
        public string DeviceId { get; set; }
        public string UsedBy { get; set; }
        public bool Granted { get; set; }
        public string DeviceUserAgent { get; set; }
        public bool Deleted { get; set; }

        protected Device() { }

        public Device(string deviceId, string deviceUserAgent, string usedBy)
        {
            DeviceId = deviceId;
            DeviceUserAgent = deviceUserAgent;
            UsedBy = usedBy;

            OnDeviceNewlyAccessed(deviceId, deviceUserAgent, usedBy);
        }

        public void Grant(string byUserId)
        {
            Granted = true;
            OnDeviceGranted(byUserId);
        }

        public void Revoke(string byUserId)
        {
            Granted = false;
            Deleted = true;
            OnDeviceRevoked(byUserId);
        }

        public void Reregister(string usedBy)
        {
            Deleted = false;
            OnDeviceReregistered(usedBy);
        }

        private void OnDeviceNewlyAccessed(string deviceId, string deviceUserAgent, string usedBy)
        {
            var ev = new DeviceNewlyAccessedEvent(this, deviceId, deviceUserAgent, usedBy);
            AddDomainEvent(ev);
        }

        private void OnDeviceGranted(string byUserId)
        {
            var ev = new DeviceGrantedEvent(this, DeviceId, byUserId);
            AddDomainEvent(ev);
        }

        private void OnDeviceRevoked(string byUserId)
        {
            var ev = new DeviceRevokedEvent(this, DeviceId, byUserId);
            AddDomainEvent(ev);
        }
        private void OnDeviceReregistered(string usedBy)
        {
            var ev = new DeviceReregisteredEvent(this, DeviceId, usedBy);
            AddDomainEvent(ev);
        }

    }
}
