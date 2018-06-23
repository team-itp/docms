using System;
using System.Collections.Generic;

namespace Docms.Web.VisualizationSystem.Models
{
    public partial class CustomerProcesses
    {
        public string UserId { get; set; }
        public string CustomerId { get; set; }
        public int FiscalYear { get; set; }
        public int Term { get; set; }
        public int? PreviousSales { get; set; }
        public int? PreviousProfit { get; set; }
        public int? SalesGoal { get; set; }
        public int? ProfitGoal { get; set; }
        public int? SalesProgress { get; set; }
        public int? ProfitProgress { get; set; }
        public string NextProcess { get; set; }
        public int Achieved { get; set; }
    }
}
