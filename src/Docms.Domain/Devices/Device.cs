using Docms.Domain.Core;
using Docms.Domain.SeedWork;
using System;

namespace Docms.Domain.Devices
{
    public class Device : Entity<DeviceId>, IAggregateRoot
    {
        public Device(string name)
        {
            var e = new DeviceAddedEvent(new DeviceId(), name ?? throw new ArgumentNullException(nameof(name)));
            AddDomainEvent(e);
        }

        public string Name { get; protected set; }
        public DateTime LastAccessedTime { get; protected set; }
        public bool IsGranted { get; protected set; }
        public bool IsRemoved { get; protected set; }

        public void Grant(UserId byUser)
        {
            if (IsGranted)
            {
                throw new InvalidOperationException("Device is already granted.");
            }
            var e = new DeviceGrantedEvent(Id, byUser);
            AddDomainEvent(e);
        }

        public void Deny(UserId byUser)
        {
            if (!IsGranted)
            {
                throw new InvalidOperationException("Device is not granted.");
            }
            var e = new DeviceDeniedEvent(Id, byUser);
            AddDomainEvent(e);
        }

        public void Accessed(UserId byUser)
        {
            var e = new DeviceAccessEvent(Id, byUser);
            AddDomainEvent(e);
        }

        public void Remove(UserId byUser)
        {
            var e = new DeviceRemovedEvent(Id, byUser);
            AddDomainEvent(e);
        }

        protected override void Apply(IDomainEvent eventItem)
        {
            switch (eventItem)
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
            IsGranted = true;
        }

        private void Apply(DeviceDeniedEvent eventItem)
        {
            IsGranted = false;
        }

        private void Apply(DeviceAccessEvent eventItem)
        {
            LastAccessedTime = eventItem.Timestamp;
        }
    }
}
