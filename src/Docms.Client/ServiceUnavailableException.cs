using System;

namespace Docms.Client
{
    internal class ServiceUnavailableException : Exception
    {
        public ServiceUnavailableException(Exception innerException) : base("サーバーが一時的に中断しているようです。しばらく停止します。", innerException)
        {
        }

        public ServiceUnavailableException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
