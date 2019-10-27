using System;
using System.Collections.Generic;

namespace Docms.Web.Identity
{
    public class ApplicationUser
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string PasswordHash { get; set; }
        public string AccountName { get; set; }
        public string DepartmentName { get; set; }
        public string TeamName { get; set; }
        public string SecurityStamp { get; set; }
        public List<string> Roles { get; } = new List<string>();
    }

    public class ApplicationRole
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
}