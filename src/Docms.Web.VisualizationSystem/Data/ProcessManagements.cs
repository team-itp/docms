using System;
using System.Collections.Generic;

namespace Docms.Web.VisualizationSystem.Data
{
    public partial class ProcessManagements
    {
        public string SalesOrderNo { get; set; }
        public string SalesPoint { get; set; }
        public bool InquiryStatusOn { get; set; }
        public bool MeetingStatusOn { get; set; }
        public bool FollowStatusOn { get; set; }
        public bool RefollowStatusOn { get; set; }
        public bool SalesAmountFixedStatusOn { get; set; }
        public string SuccessCause { get; set; }
        public string LostCause { get; set; }
    }
}
