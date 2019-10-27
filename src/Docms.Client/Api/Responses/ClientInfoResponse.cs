using System;

namespace Docms.Client.Api.Responses
{
    public class ClientInfoResponse
    {
        public string ClientId { get; set; }
        public string Type { get; set; }
        public string IpAddress { get; set; }
        public string Status { get; set; }
        public DateTime? LastAccessedTime { get; set; }

        public string RequestId { get; set; }
        public string RequestType { get; set; }
        public DateTime? RequestedAt { get; set; }
        public string AcceptedRequestId { get; set; }
        public string AcceptedRequestType { get; set; }
        public DateTime? AcceptedAt { get; set; }

        public string LastMessage { get; set; }
    }
}
