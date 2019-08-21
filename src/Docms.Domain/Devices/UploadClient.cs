using Docms.Domain.SeedWork;

namespace Docms.Domain.Devices
{
    public class UploadClient : Device
    {
        public UploadClientStatus Status { get; protected set; }

        public UploadClient() : base(Constants.UPLOAD_CLIENT)
        {
            Status = UploadClientStatus.Stopped;
        }

        public void Starting()
        {
            var e = new UploadClientStartingEvent(Id);
            AddDomainEvent(e);
        }

        public void Started()
        {
            var e = new UploadClientStartedEvent(Id);
            AddDomainEvent(e);
        }

        public void Stopping()
        {
            var e = new UploadClientStoppingEvent(Id);
            AddDomainEvent(e);
        }

        public void Stopped()
        {
            var e = new UploadClientStoppedEvent(Id);
            AddDomainEvent(e);
        }

        protected override void Apply(IDomainEvent eventItem)
        {
            switch (eventItem)
            {
                case UploadClientStartingEvent e:
                    Apply(e);
                    break;
                case UploadClientStartedEvent e:
                    Apply(e);
                    break;
                case UploadClientStoppedEvent e:
                    Apply(e);
                    break;
                case UploadClientStoppingEvent e:
                    Apply(e);
                    break;
            }
        }

        private void Apply(UploadClientStartingEvent eventItem)
        {
            Status = UploadClientStatus.Starting;
        }

        private void Apply(UploadClientStartedEvent eventItem)
        {
            Status = UploadClientStatus.Running;
        }

        private void Apply(UploadClientStoppingEvent eventItem)
        {
            Status = UploadClientStatus.Stopping;
        }

        private void Apply(UploadClientStoppedEvent eventItem)
        {
            Status = UploadClientStatus.Stopped;
        }
    }
}
