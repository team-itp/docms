using Docms.Client.Types;

namespace Docms.Client.Synchronization
{
    public class RequestForUploadState : SynchronizationState
    {
        private readonly int _retryCount;

        public RequestForUploadState(PathString path, string hash, long length) : base(path, hash, length)
        {
            _retryCount = 0;
        }

        protected RequestForUploadState(PathString path, string hash, long length, int retryCount) : base(path, hash, length)
        {
            _retryCount = retryCount;
        }

        public SynchronizationState Uploaded()
        {
            return new UploadingState(Path, Hash, Length);
        }

        public SynchronizationState Failed()
        {
            if (_retryCount > 3)
            {
                return new UploadRequestFailedState(Path, Hash, Length);
            }
            else
            {
                return new RequestForUploadState(Path, Hash, Length, _retryCount + 1);
            }
        }
    }
}
