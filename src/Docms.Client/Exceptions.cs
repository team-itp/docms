using System;

namespace Docms.Client
{
    public class DocmsClientException : Exception
    {
        public DocmsClientException()
        {
        }

        public DocmsClientException(string message) : base(message)
        {
        }

        public DocmsClientException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    public class InvalidLoginException : DocmsClientException
    {
        public InvalidLoginException() : base("ログインができません。ユーザー名とパスワードを確認してください。")
        {
        }
    }

    public class TokenVerificationException : DocmsClientException
    {
        public TokenVerificationException() : base("トークンの認証に失敗しました。一度ログアウトして再度ログインしてください。")
        {
        }
    }

    public class ServerException : DocmsClientException
    {
        public string Content { get; }

        public ServerException(int httpStatusCode, string content) : base("サーバーとの接続に問題があるようです。しばらくたってからやり直してください。ステータスコード:" + httpStatusCode.ToString())
        {
            Content = content;
        }
    }
}
