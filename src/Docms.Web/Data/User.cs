using System.Collections.Generic;

namespace Docms.Web.Data
{
    public class User
    {
        public User()
        {
            Metadata = new List<UserMeta>();
        }

        public int Id { get; set; }
        public string VSUserId { get; set; }

        public virtual ICollection<UserMeta> Metadata { get; set; }
    }

    public class UserMeta
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string MetaKey { get; set; }
        public string MetaValue { get; set; }
    }
}
