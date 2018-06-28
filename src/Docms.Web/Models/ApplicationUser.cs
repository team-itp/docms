using System;

namespace Docms.Web.Models
{
    public class ApplicationUser
    {
        public ApplicationUser(string accountName)
        {
            AccountName = accountName;
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public string AccountName { get; set; }
        public string PasswordHash { get; set; }
        public int Rank { get; set; }
        public int? Department { get; set; }
        public string TeamId { get; set; }
    }

    public class ApplicationRole
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }
}
