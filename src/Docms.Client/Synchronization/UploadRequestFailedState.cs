using Docms.Client.Types;

namespace Docms.Client.Synchronization
{
    public class UploadRequestFailedState : SynchronizationState
    {
        public UploadRequestFailedState(PathString path, string hash, long length) : base(path, hash, length)
        {
        }
    }
}