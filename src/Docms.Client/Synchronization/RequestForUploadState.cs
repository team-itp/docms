using Docms.Client.Types;

namespace Docms.Client.Synchronization
{
    public class RequestForUploadState : SynchronizationState
    {
        public RequestForUploadState(PathString path, string hash, long length) : base(path, hash, length)
        {
        }

        public SynchronizationState Uploaded()
        {
            return new UploadingState(Path, Hash, Length);
        }
    }
}
