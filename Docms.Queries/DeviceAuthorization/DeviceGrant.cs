using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Docms.Queries.DeviceAuthorization
{
    public class DeviceGrant
    {
        [Column("DeviceId")]
        [Key]
        public string DeviceId { get; set; }
        [Column("GrantedBy")]
        public string GrantedBy { get; set; }
    }
}
