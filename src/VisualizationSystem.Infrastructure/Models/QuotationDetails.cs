using System;
using System.Collections.Generic;

namespace VisualizationSystem.Infrastructure.Models
{
    public partial class QuotationDetails
    {
        public string QuotationNo { get; set; }
        public int No { get; set; }
        public int? DetailType { get; set; }
        public string Name { get; set; }
        public string Spec { get; set; }
        public string Unit { get; set; }
        public double? Count { get; set; }
        public int? UnitPrice { get; set; }
        public int? Price { get; set; }
        public string Remark { get; set; }
    }
}
