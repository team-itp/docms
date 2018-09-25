using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

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