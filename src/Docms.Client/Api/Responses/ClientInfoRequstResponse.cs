using System;

namespace Docms.Client.Api.Responses
{
    public class ClientInfoRequstResponse
    {
        public string RequestId { get; set; }
        public string RequestType { get; set; }
        public DateTime? RequestedAt { get; set; }
    }
}
