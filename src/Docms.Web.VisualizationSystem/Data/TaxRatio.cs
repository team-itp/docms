using System;
using System.Collections.Generic;

namespace Docms.Web.VisualizationSystem.Data
{
    public partial class TaxRatio
    {
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public int Ratio { get; set; }
    }
}
