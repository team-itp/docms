using System;

namespace Docms.Client.Exceptions
{
    public class ServerException : Exception
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