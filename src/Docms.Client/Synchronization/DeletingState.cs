using Docms.Client.Types;

namespace Docms.Client.Synchronization
{
    public class DeletingState : SynchronizationState
    {
        public DeletingState(PathString path, string hash, long length) : base(path, hash, length)
        {
        }
    }
}