using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Docms.Queries.DeviceAuthorization
{
    public class DeviceGrant
    {
        [Column("DeviceId")]
        [Key]
        public string DeviceId { get; set; }
        [Column("IsGranted")]
        public bool IsGranted { get; set; }
        [Column("GrantedBy")]
        public string GrantedBy { get; set; }
        [Column("GrantedAt")]
        public DateTime? GrantedAt { get; set; }
    }
}
