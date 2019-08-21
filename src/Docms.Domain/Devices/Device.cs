using Docms.Domain.Core;
using Docms.Domain.SeedWork;
using Docms.Domain.Users;
using System;

namespace Docms.Domain.Devices
{
    public class Device : Entity<DeviceId>, IAggregateRoot
    {
        public Device(string name)
        {
            var e = new DeviceAddedEvent(new DeviceId(), name ?? throw new ArgumentNullException(nameof(name)));
            Apply(e);
            AddDomainEvent(e);
        }

        public string Name { get; protected set; }
        public UserId GrantedBy { get; protected set; }
        public bool IsGranted => GrantedBy != null;
        public DateTime LastAccessedTime { get; protected set; }

        public void Grant(IUser byUser)
        {
            if (IsGranted)
            {
                throw new InvalidOperationException("Device is already granted.");
            }
            var e = new DeviceGrantedEvent(Id, byUser.Id);
            Apply(e);
            AddDomainEvent(e);
        }

        public void Deny(IUser byUser)
        {
            if (!IsGranted)
            {
                throw new InvalidOperationException("Device is not granted.");
            }
            var e = new DeviceDeniedEvent(Id, byUser.Id);
            Apply(e);
            AddDomainEvent(e);
        }

        public void Accessed()
        {
            var e = new DeviceAccessEvent(Id);
            Apply(e);
            AddDomainEvent(e);
        }

        protected override void Apply(IDomainEvent eventItem)
        {
            switch(eventItem)
            {
                case DeviceAddedEvent e:
                    Apply(e);
                    break;
                case DeviceGrantedEvent e:
                    Apply(e);
                    break;
                case DeviceDeniedEvent e:
                    Apply(e);
                    break;
                case DeviceAccessEvent e:
                    Apply(e);
                    break;
            }
        }

        private void Apply(DeviceAddedEvent eventItem)
        {
            Id = eventItem.DeviceId;
            Name = eventItem.DeviceName;
        }

        private void Apply(DeviceGrantedEvent eventItem)
        {
            GrantedBy = eventItem.GrantedBy;
        }

        private void Apply(DeviceDeniedEvent eventItem)
        {
            GrantedBy = null;
        }

        private void Apply(DeviceAccessEvent eventItem)
        {
            LastAccessedTime = eventItem.Timestamp;
        }
    }
}
