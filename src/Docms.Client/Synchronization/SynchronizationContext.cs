using Docms.Client.Types;
using System;
using System.Collections.Generic;

namespace Docms.Client.Synchronization
{
    public class SynchronizationContext
    {
        public IEnumerable<SynchronizationState> States => _States.Values;
        private Dictionary<PathString, SynchronizationState> _States;

        public SynchronizationContext()
        {
            _States = new Dictionary<PathString, SynchronizationState>();
        }

        public void AddLocalFile(PathString path, string hash, long length)
        {
            _States.Add(path, new RequestForUploadState(path, hash, length));
        }

        public void LocalFileUploaded(PathString path)
        {
            if (_States[path] is RequestForUploadState up)
            {
                _States[path] = up.Uploaded();
            }
        }

        public void AddRemoteFile(PathString path, string hash, long length)
        {
            if (_States.TryGetValue(path, out var value))
            {
                if (value is UploadingState)
                {
                    _States.Remove(path);
                }
            }
            else
            {
                _States.Add(path, new RequestForDownloadState(path, hash, length));
            }
        }

        public void RemoteFileDownloaded(PathString path)
        {
            if (_States[path] is RequestForDownloadState down)
            {
                _States[path] = down.Downloaded();
            }
        }
    }
}
