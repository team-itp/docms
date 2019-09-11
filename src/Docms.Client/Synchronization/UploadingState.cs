using Docms.Client.Types;

namespace Docms.Client.Synchronization
{
    public class UploadingState : SynchronizationState
    {
        public UploadingState(PathString path, string hash, long length) : base(path, hash, length)
        {
        }
    }
}
