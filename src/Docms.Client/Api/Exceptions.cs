﻿using System;

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

    public class InvalidLoginException : DocmsApiClientException
    {
        public InvalidLoginException() : base("ログインができません。ユーザー名とパスワードを確認してください。")
        {
        }
    }

    public class TokenVerificationException : DocmsApiClientException
    {
        public TokenVerificationException() : base("トークンの認証に失敗しました。一度ログアウトして再度ログインしてください。")
        {
        }
    }

    public class ServerException : DocmsApiClientException
    {
        public string Content { get; }

        public ServerException(int httpStatusCode, string content) : base("サーバーとの接続に問題があるようです。しばらくたってからやり直してください。ステータスコード:" + httpStatusCode.ToString())
        {
            Content = content;
        }
    }
}