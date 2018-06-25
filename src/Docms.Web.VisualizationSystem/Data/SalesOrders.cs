using System;
using System.Collections.Generic;

namespace Docms.Web.VisualizationSystem.Data
{
    public partial class SalesOrders
    {
        public string No { get; set; }
        public string RelatedNo { get; set; }
        public string Name { get; set; }
        public int FiscalYear { get; set; }
        public int TaxRatio { get; set; }
        public bool QuotationStatusOn { get; set; }
        public bool OrderStatusOn { get; set; }
        public bool CompleteStatusOn { get; set; }
        public bool InvoiceStatusOn { get; set; }
        public bool ReceiptStatusOn { get; set; }
        public bool LostStatusOn { get; set; }
        public string Place { get; set; }
        public int? SalesAmount { get; set; }
        public int? InvoiceAmount { get; set; }
        public int? ReceiptAmount { get; set; }
        public int? PurchaseAmountOfHumanResource { get; set; }
        public int? PurchaseAmountOfMaterial { get; set; }
        public int? LaborCostAmount { get; set; }
        public int? CostAmount { get; set; }
        public int? ExpenseAmount { get; set; }
        public string CustomerName { get; set; }
        public string CustomerId { get; set; }
        public string CustomerAddress { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? StartDate2 { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? EndDate2 { get; set; }
        public DateTime? ScheduledInvoiceDate { get; set; }
        public DateTime? ScheduledReceiptDate { get; set; }
        public string DepartmentName { get; set; }
        public string UserIdOfPersonInCharge { get; set; }
        public string PersonInCharge { get; set; }
        public string CustomerPersonInCharge { get; set; }
        public string WorkType { get; set; }
        public string OtherWorkType { get; set; }
        public string PaymentCondition { get; set; }
        public DateTime? ScheduledQuotationDate { get; set; }
        public string Remark { get; set; }
        public bool InAdvance { get; set; }
        public bool BasedOnAchievement { get; set; }
    }
}
