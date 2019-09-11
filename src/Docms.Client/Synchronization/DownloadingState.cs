using Docms.Client.Types;

namespace Docms.Client.Synchronization
{
    public class DownloadingState : SynchronizationState
    {
        public DownloadingState(PathString path, string hash, long length) : base(path, hash, length)
        {
        }
    }
}
