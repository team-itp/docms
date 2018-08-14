using System;

namespace Docms.Web.Application.Identity
{
    public class ApplicationUser
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string PasswordHash { get; set; }
        public string AccountName { get; set; }
        public string DepartmentName { get; set; }
        public string TeamName { get; internal set; }
    }

    public class ApplicationRole
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }
}