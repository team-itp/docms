using System;

namespace Docms.Client.Exceptions
{
    public class ApplicationNeedsReinitializeException : Exception
    {
        public ApplicationNeedsReinitializeException(Exception innerException) : base("アプリが不安定となっているため、一時データを削除して再度実行しなおします。", innerException)
        {
        }

        public ApplicationNeedsReinitializeException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
