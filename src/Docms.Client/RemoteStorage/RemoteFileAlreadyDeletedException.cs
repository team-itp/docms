using System;
using System.Runtime.Serialization;
using Docms.Client.SeedWork;

namespace Docms.Client.RemoteStorage
{
    [Serializable]
    public class RemoteFileAlreadyDeletedException : Exception
    {
        public PathString Path { get; }

        public RemoteFileAlreadyDeletedException(PathString path)
        {
            Path = path;
        }
    }
}