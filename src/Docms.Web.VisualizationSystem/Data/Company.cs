using System;
using System.Collections.Generic;

namespace Docms.Web.VisualizationSystem.Data
{
    public partial class Company
    {
        public string Name { get; set; }
        public int PostalCode1 { get; set; }
        public int PostalCode2 { get; set; }
        public double Address { get; set; }
        public double Latitude { get; set; }
        public string Longitude { get; set; }
        public int ClosingMonth { get; set; }
        public int StartYear { get; set; }
    }
}
