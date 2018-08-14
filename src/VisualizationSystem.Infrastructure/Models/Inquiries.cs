using System;
using System.Collections.Generic;

namespace VisualizationSystem.Infrastructure.Models
{
    public partial class Inquiries
    {
        public string SalesOrderNo { get; set; }
        public DateTime? InquiryDate { get; set; }
        public DateTime? MeetingDate { get; set; }
        public DateTime? QuotationDate { get; set; }
        public DateTime? FollowDate { get; set; }
        public DateTime? RefollowDate { get; set; }
        public DateTime? SalesAmountFixedDate { get; set; }
        public DateTime? SuccessDate { get; set; }
        public DateTime? LostDate { get; set; }
        public string SuccessCause { get; set; }
        public string LostCause { get; set; }
        public int? ProfitGoal { get; set; }
        public double? ProfitRatioGoal { get; set; }
    }
}
