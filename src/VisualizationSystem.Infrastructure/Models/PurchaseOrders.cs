using System;
using System.Collections.Generic;

namespace VisualizationSystem.Infrastructure.Models
{
    public partial class PurchaseOrders
    {
        public string No { get; set; }
        public string SalesOrderNo { get; set; }
        public string Name { get; set; }
        public int TaxRatio { get; set; }
        public bool OrderStatusOn { get; set; }
        public bool CompleteStatusOn { get; set; }
        public bool InvoiceStatusOn { get; set; }
        public bool PaymentStatusOn { get; set; }
        public int PurchaseType { get; set; }
        public string SupplierId { get; set; }
        public string SupplierName { get; set; }
        public int? Amount { get; set; }
        public int? InvoiceAmount { get; set; }
        public int? PaymentAmount { get; set; }
        public string Content { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? EndDate2 { get; set; }
        public DateTime? PaymentDate { get; set; }
        public string Remark { get; set; }
    }
}
