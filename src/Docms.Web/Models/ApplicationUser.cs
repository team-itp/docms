﻿using System;

namespace Docms.Web.Models
{
    public class ApplicationUser
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string AccountName { get; set; }
        public string PasswordHash { get; set; }
    }

    public class ApplicationRole
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }
}
