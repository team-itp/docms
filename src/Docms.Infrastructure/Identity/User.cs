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

    public class DocmsUserRole
    {
        [Column("UserId")]
        public string UserId { get; set; }

        [Column("Role")]
        public string Role { get; set; }
    }
}
