using System;
using System.Collections.Generic;

namespace Docms.Web.VisualizationSystem.Data
{
    public partial class Costs
    {
        public string SalesOrderNo { get; set; }
        public int No { get; set; }
        public DateTime? Date { get; set; }
        public string CostType { get; set; }
        public int? Price { get; set; }
        public string Remark { get; set; }
    }
}
