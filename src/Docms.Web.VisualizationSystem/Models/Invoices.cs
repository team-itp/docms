using System;
using System.Collections.Generic;

namespace Docms.Web.VisualizationSystem.Models
{
    public partial class Invoices
    {
        public string No { get; set; }
        public string SalesOrderNo { get; set; }
        public string Spec { get; set; }
        public string TradeCondition { get; set; }
        public DateTime? ScheduledInvoiceDate { get; set; }
        public DateTime? ScheduledReceiptDate { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public DateTime? ReceiptDate { get; set; }
        public bool InvoiceStatusOn { get; set; }
        public bool ReceiptStatusOn { get; set; }
        public int? Amount { get; set; }
        public int? ReceiptAmount { get; set; }
        public bool Obsolete { get; set; }
        public bool BasedOnAchievement { get; set; }
        public bool Remain { get; set; }
    }
}
