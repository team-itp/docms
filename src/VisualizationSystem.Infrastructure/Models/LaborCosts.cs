using System;
using System.Collections.Generic;

namespace VisualizationSystem.Infrastructure.Models
{
    public partial class LaborCosts
    {
        public string SalesOrderNo { get; set; }
        public int No { get; set; }
        public DateTime? Date { get; set; }
        public string Worker { get; set; }
        public int? Price { get; set; }
        public string Remark { get; set; }
    }
}
