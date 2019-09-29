using System;

namespace Docms.Client.Exceptions
{
    internal class ServiceUnavailableException : Exception
    {
        public ServiceUnavailableException() : base("現在サーバーに接続できません。")
        {
        }

        public ServiceUnavailableException(Exception innerException) : base("現在サーバーに接続できません。", innerException)
        {
        }

        public ServiceUnavailableException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
