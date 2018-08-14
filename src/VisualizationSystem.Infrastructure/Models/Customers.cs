using System;
using System.Collections.Generic;

namespace VisualizationSystem.Infrastructure.Models
{
    public partial class Customers
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Kana { get; set; }
        public string PostalCode1 { get; set; }
        public string PostalCode2 { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string Tel1 { get; set; }
        public string Tel2 { get; set; }
        public string Tel3 { get; set; }
        public string Fax1 { get; set; }
        public string Fax2 { get; set; }
        public string Fax3 { get; set; }
        public int BusinessType { get; set; }
        public string UserIdOfPersonInCharge { get; set; }
        public int? DayOfClosing { get; set; }
        public int? DayOfArriving { get; set; }
        public int? DayOfPayment { get; set; }
        public string Remark { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }
}
