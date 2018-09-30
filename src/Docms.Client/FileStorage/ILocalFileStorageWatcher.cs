using System;

namespace Docms.Client.FileStorage
{
    public interface ILocalFileStorageWatcher
    {
        event EventHandler<FileCreatedEventArgs> FileCreated;
        event EventHandler<FileModifiedEventArgs> FileModified;
        event EventHandler<FileDeletedEventArgs> FileDeleted;
        event EventHandler<FileMovedEventArgs> FileMoved;
    }
}