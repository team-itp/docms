using System;
using System.Runtime.Serialization;
using Docms.Client.SeedWork;

namespace Docms.Client.RemoteStorage
{
    [Serializable]
    public class RetryTimeoutException : Exception
    {
        public PathString Path { get; }

        private int retryCount;

        public RetryTimeoutException(PathString path, int retryCount)
        {
            Path = path;
            this.retryCount = retryCount;
        }

        public RetryTimeoutException(PathString path, int retryCount, Exception innerException) : base(null, innerException)
        {
            Path = path;
            this.retryCount = retryCount;
        }
    }
}