using System;
using System.Threading;
using System.Threading.Tasks;

namespace Docms.Client.FileWatching
{
    public interface ILocalFileStorageWatcher
    {
        event EventHandler<FileCreatedEventArgs> FileCreated;
        event EventHandler<FileModifiedEventArgs> FileModified;
        event EventHandler<FileDeletedEventArgs> FileDeleted;
        event EventHandler<FileMovedEventArgs> FileMoved;

        Task StartWatch(CancellationToken cancellationToken);
        Task StopWatch(bool nowait = true);
    }
}