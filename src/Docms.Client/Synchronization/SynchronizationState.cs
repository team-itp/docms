using System;
using Docms.Client.Types;

namespace Docms.Client.Synchronization
{
    public abstract class SynchronizationState
    {
        protected SynchronizationState(PathString path, string hash, long length)
        {
            Path = path;
            Hash = hash;
            Length = length;
        }

        public PathString Path { get; }
        public string Hash { get; }
        public long Length { get; }
    }
}
