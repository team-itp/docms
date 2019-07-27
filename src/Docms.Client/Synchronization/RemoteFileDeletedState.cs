using Docms.Client.Types;

namespace Docms.Client.Synchronization
{
    public class RemoteFileDeletedState : SynchronizationState
    {
        public RemoteFileDeletedState(PathString path, string hash, long length) : base(path, hash, length)
        {
        }
    }
}
