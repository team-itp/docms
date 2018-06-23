using System;
using System.Collections.Generic;

namespace Docms.Web.VisualizationSystem.Models
{
    public partial class Payments
    {
        public string PurchaseOrderNo { get; set; }
        public int No { get; set; }
        public int? InvoiceAmount { get; set; }
        public int? PaymentAmount { get; set; }
        public DateTime? ScheduledPaymentDate { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public DateTime? PaymentDate { get; set; }
        public bool InvoiceStatusOn { get; set; }
        public bool PaymentStatusOn { get; set; }
    }
}
