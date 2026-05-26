using System;

namespace HVIP.Models
{
    public class ShippingSettings
    {
        public int      Id            { get; set; } = 1;
        public decimal  FreeThreshold { get; set; } = 500m;
        public decimal  Charge        { get; set; } = 60m;
        public DateTime UpdatedOn     { get; set; }
    }
}
