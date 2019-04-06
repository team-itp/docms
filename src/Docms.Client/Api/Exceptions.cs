using System;

namespace Docms.Client.Api
{
    public class DocmsApiClientException : Exception
    {
        public DocmsApiClientException()
        {
        }

        public DocmsApiClientException(string message) : base(message)
        {
        }

        public DocmsApiClientException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    public class ServerConnectionException : DocmsApiClientException
    {
        public ServerConnectionException() : base($"サーバーに接続できません。")
        {
        }
    }

    public class InvalidLoginException : DocmsApiClientException
    {
        public InvalidLoginException() : base("ログインができません。ユーザー名とパスワードを確認してください。")
        {
        }
    }

    public class ServerException : DocmsApiClientException
    {
        public string RequestUri { get; }
        public string RequestMethod { get; }
        public string RequestContent { get; }
        public int StatusCode { get; }
        public string Content { get; }

        public ServerException(string requestUri, string requestMethod, string requestContent, int httpStatusCode, string content) : base($"サーバーとの接続に問題があるようです。しばらくたってからやり直してください。\n\tリソース:{requestUri}\n\tメソッド:{requestMethod}\n\tリクエスト:{requestContent}\n\tステータスコード:{httpStatusCode}")
        {
            RequestUri = requestUri;
            RequestMethod = requestMethod;
            RequestContent = requestContent;
            StatusCode = httpStatusCode;
            Content = content;
        }

        public ServerException(string content, Exception innerException) : base("サーバーとの通信でエラーが発生しました。", innerException)
        {
            Content = content;
        }
    }
}