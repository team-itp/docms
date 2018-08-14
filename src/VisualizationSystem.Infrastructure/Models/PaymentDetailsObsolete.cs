using System;
using System.Collections.Generic;

namespace VisualizationSystem.Infrastructure.Models
{
    public partial class PaymentDetailsObsolete
    {
        public string PurchaseOrderNo { get; set; }
        public int No { get; set; }
        public string Content { get; set; }
        public int? Price { get; set; }
    }
}
