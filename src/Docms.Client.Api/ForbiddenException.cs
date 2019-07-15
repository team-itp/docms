using System;
using System.Collections.Generic;
using System.Text;

namespace Docms.Client.Api
{
    public class ForbiddenException : DocmsApiException
    {
        public ForbiddenException() : base("リクエストが拒否されました。")
        {
        }
    }
}
