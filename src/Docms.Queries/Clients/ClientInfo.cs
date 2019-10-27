using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Docms.Queries.Clients
{
    public class ClientInfo
    {
        [Column("ClientId")]
        [Key]
        public string ClientId { get; set; }
        [Column("Type")]
        public string Type { get; set; }
        [Column("IpAddress")]
        public string IpAddress { get; set; }
        [Column("Status")]
        public string Status { get; set; }
        [Column("LastAccessedTime")]
        public DateTime? LastAccessedTime { get; set; }

        [Column("RequestId")]
        public string RequestId { get; set; }
        [Column("RequestType")]
        public string RequestType { get; set; }
        [Column("RequestedAt")]
        public DateTime? RequestedAt { get; set; }
        [Column("AcceptedRequestId")]
        public string AcceptedRequestId { get; set; }
        [Column("AcceptedRequestType")]
        public string AcceptedRequestType { get; set; }
        [Column("AcceptedAt")]
        public DateTime? AcceptedAt { get; set; }

        [Column("LastMessage")]
        public string LastMessage { get; set; }
    }
}
