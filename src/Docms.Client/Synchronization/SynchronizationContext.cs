using Docms.Client.Types;
using System;
using System.Collections.Generic;

namespace Docms.Client.Synchronization
{
    public class SynchronizationContext
    {
        public IEnumerable<SynchronizationState> States => _States.Values;
        private readonly Dictionary<PathString, SynchronizationState> _States;

        public SynchronizationContext()
        {
            _States = new Dictionary<PathString, SynchronizationState>();
        }

        public void LocalFileAdded(PathString path, string hash, long length)
        {
            if (_States.TryGetValue(path, out var value))
            {
                if ((value is DownloadingState || value is RemoteFileDeletedState || value is RequestForDeleteState)
                    && value.Hash == hash
                    && value.Length == length)
                {
                    _States.Remove(path);
                }
                else
                {
                    _States[path] = new RequestForUploadState(path, hash, length);
                }
            }
            else
            {
                _States.Add(path, new RequestForUploadState(path, hash, length));
            }
        }

        public void LocalFileDeleted(PathString path, string hash, long length)
        {
            if (_States.TryGetValue(path, out var value))
            {
                if (value is UploadingState)
                {
                    _States[path] = new RequestForDeleteState(path, hash, length);
                }
                else
                {
                    _States.Remove(path);
                }
            }
            else
            {
                _States.Add(path, new RequestForDeleteState(path, hash, length));
            }
        }

        public void RemoteFileAdded(PathString path, string hash, long length)
        {
            if (_States.TryGetValue(path, out var value))
            {

                if (value.Hash == hash && value.Length == length)
                {
                    _States.Remove(path);
                }
                else if (value is UploadingState || value is RequestForDownloadState)
                {
                    _States[path] = new RequestForDownloadState(path, hash, length);
                }
            }
            else
            {
                _States.Add(path, new RequestForDownloadState(path, hash, length));
            }
        }

        public void RemoteFileDeleted(PathString path, string hash, long length)
        {
            if (_States.TryGetValue(path, out var value))
            {
                if (value is DeletingState || value is RequestForDownloadState)
                {
                    _States.Remove(path);
                }
            }
            else
            {
                _States.Add(path, new RemoteFileDeletedState(path, hash, length));
            }
        }

        public void UploadRequested(PathString path)
        {
            if (_States.TryGetValue(path, out var state) && state is RequestForUploadState up)
            {
                _States[path] = up.Uploaded();
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        public void DownloadRequested(PathString path)
        {
            if (_States.TryGetValue(path, out var state) && state is RequestForDownloadState down)
            {
                _States[path] = down.Downloaded();
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        public void DeleteRequested(PathString path)
        {
            if (_States.TryGetValue(path, out var state) && state is RequestForDeleteState del)
            {
                _States[path] = del.Deleted();
            }
            else
            {
                throw new InvalidOperationException();
            }
        }
    }
}
