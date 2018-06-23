using System;
using System.Collections.Generic;

namespace Docms.Web.VisualizationSystem.Models
{
    public partial class IssuedQuotations
    {
        public string No { get; set; }
        public string SalesOrderNo { get; set; }
        public DateTime? LimitedDate { get; set; }
        public DateTime? IssueDate { get; set; }
        public byte[] Document { get; set; }
    }
}
