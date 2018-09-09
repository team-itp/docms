using System;
using Docms.Domain.Events.Identity;
using Docms.Domain.SeedWork;

namespace Docms.Domain.Identity
{
    public class Device : Entity, IAggregateRoot
    {
        public string DeviceId { get; set; }
        public bool Granted { get; set; }

        protected Device() { }

        public Device(string deviceId)
        {
            DeviceId = deviceId;

            OnDeviceNewlyAccessed(deviceId);
        }

        public void Grant(string byUserId)
        {
            Granted = true;
            OnDeviceGranted(byUserId);
        }

        public void Revoke(string byUserId)
        {
            Granted = false;
            OnDeviceRevoked(byUserId);
        }

        private void OnDeviceNewlyAccessed(string deviceId)
        {
            var ev = new DeviceNewlyAccessedEvent(this, deviceId);
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
    }
}
