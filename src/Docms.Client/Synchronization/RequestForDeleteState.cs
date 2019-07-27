using Docms.Client.Types;

namespace Docms.Client.Synchronization
{
    public class RequestForDeleteState : SynchronizationState
    {
        public RequestForDeleteState(PathString path, string hash, long length) : base(path, hash, length)
        {
        }

        public SynchronizationState Deleted()
        {
            return new DeletingState(Path, Hash, Length);
        }
    }
}
