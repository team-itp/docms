using System;
using System.Collections.Generic;

namespace Docms.Web.VisualizationSystem.Models
{
    public partial class Expenses
    {
        public string SalesOrderNo { get; set; }
        public int? Subcontract { get; set; }
        public int? Transportation { get; set; }
        public int? Entertainment { get; set; }
        public int? Supplies { get; set; }
        public int? Labor { get; set; }
    }
}
