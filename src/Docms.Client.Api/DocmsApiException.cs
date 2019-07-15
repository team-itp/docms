using System;

namespace Docms.Client.Api
{
    public class DocmsApiException : Exception
    {
        public DocmsApiException(string message) : base(message)
        {
        }

        public DocmsApiException(Exception innerException) : base("リクエストの処理中にエラーが発生しました。", innerException)
        {
        }
    }
}
