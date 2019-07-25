using Docms.Client.Types;

namespace Docms.Client.Synchronization
{
    public class RequestForDownloadState : SynchronizationState
    {
        public RequestForDownloadState(PathString path, string hash, long length) : base(path, hash, length)
        {
        }

        public SynchronizationState Downloaded()
        {
            return new DownloadingState(Path, Hash, Length);
        }
    }
}
