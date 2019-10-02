using System;

namespace Docms.Client.Exceptions
{
    public class InvalidLoginException : Exception
    {
        public InvalidLoginException() : base("ログインができません。ユーザー名とパスワードを確認してください。")
        {
        }
    }
}
