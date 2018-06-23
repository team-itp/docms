using System;
using System.Collections.Generic;

namespace Docms.Web.VisualizationSystem.Models
{
    public partial class Materials
    {
        public string PurchaseOrderNo { get; set; }
        public int No { get; set; }
        public string Name { get; set; }
        public string Color { get; set; }
        public string Volume { get; set; }
        public double? Count { get; set; }
        public int? UnitPrice { get; set; }
        public int? Price { get; set; }
    }
}
