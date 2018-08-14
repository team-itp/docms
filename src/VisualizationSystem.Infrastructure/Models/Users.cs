using System;
using System.Collections.Generic;

namespace VisualizationSystem.Infrastructure.Models
{
    public partial class Users
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string AccountName { get; set; }
        public string Password { get; set; }
        public int Rank { get; set; }
        public int? Department { get; set; }
        public string TeamId { get; set; }
        public int Theme { get; set; }
        public bool Obsolete { get; set; }
    }
}
