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

        public void LocalFileAdded(PathString path, string hash, long length)
        {
            if (_States.TryGetValue(path, out var value))
            {
                if (value is DownloadingState down && down.Hash == hash && down.Length == length)
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
                _States.Remove(path);
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
                if (value is UploadingState up)
                {
                    if (up.Hash == hash && up.Length == length)
                    {
                        _States.Remove(path);
                    }
                    else
                    {
                        _States[path] = new RequestForDownloadState(path, hash, length);
                    }
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
                _States.Remove(path);
            }
        }

        public void UploadRequested(PathString path)
        {
            if (_States[path] is RequestForUploadState up)
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
            if (_States[path] is RequestForDownloadState down)
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
            if (_States[path] is RequestForDeleteState del)
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
