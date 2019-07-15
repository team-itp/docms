namespace Docms.Client.Api
{
    public class UnauthorizedException : DocmsApiException
    {
        public UnauthorizedException() : base("サーバーにログイン出来ません。")
        {
        }
    }
}
