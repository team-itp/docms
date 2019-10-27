using System.Collections.Generic;

namespace Docms.Queries.Identity
{
    public class User
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string AccountName { get; set; }
        public string DepartmentName { get; set; }
        public string TeamName { get; set; }
        public string SecurityStamp { get; set; }

        public string Password { get; set; }
        public List<string> Roles { get; set; }
    }
}
