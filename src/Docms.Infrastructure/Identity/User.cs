using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Docms.Infrastructure.Identity
{
    public class DocmsUser
    {
        [Column("Id")]
        [Key]
        public string Id { get; set; }
        [Column("SecurityStamp")]
        public string SecurityStamp { get; set; }
    }
}
