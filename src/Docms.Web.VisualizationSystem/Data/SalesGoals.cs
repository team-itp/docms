using System;
using System.Collections.Generic;

namespace Docms.Web.VisualizationSystem.Data
{
    public partial class SalesGoals
    {
        public string UserId { get; set; }
        public string CustomerId { get; set; }
        public int SalesGoal { get; set; }
        public int ProfitGoal { get; set; }
    }
}
