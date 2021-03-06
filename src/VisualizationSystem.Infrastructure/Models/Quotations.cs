﻿using System;
using System.Collections.Generic;

namespace VisualizationSystem.Infrastructure.Models
{
    public partial class Quotations
    {
        public string No { get; set; }
        public string SalesOrderNo { get; set; }
        public DateTime? LimitedDate { get; set; }
        public DateTime? IssueDate { get; set; }
    }
}
