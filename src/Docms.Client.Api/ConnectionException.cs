namespace Docms.Client.Api
{
    public class ConnectionException : DocmsApiException
    {
        public ConnectionException(string requestUrl) : base("サーバーに接続できません。URL: " + requestUrl)
        {
        }
    }
}
