using System;
using System.Collections.Generic;

namespace Docms.Web.VisualizationSystem.Models
{
    public partial class Histories
    {
        public string Id { get; set; }
        public DateTime DateTime { get; set; }
        public string UserName { get; set; }
        public string Operation { get; set; }
        public string Reason { get; set; }
    }
}
