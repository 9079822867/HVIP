using System.Collections.Generic;
using System.Linq;
using HVIP.Helpers;

namespace HVIP.Models
{
    public class CartItem
    {
        public int     ProductId     { get; set; }
        public string  ProductName   { get; set; }
        public string  ProductSize   { get; set; }
        public string  CategoryIcon  { get; set; }
        public string  CategoryColor { get; set; }
        public decimal UnitPrice     { get; set; }
        public int     Quantity      { get; set; }
        public decimal Total         => UnitPrice * Quantity;
    }

    public class Cart
    {
        public List<CartItem> Items { get; set; } = new List<CartItem>();

        public decimal SubTotal      => Items.Sum(i => i.Total);
        public decimal Shipping      => ShippingConfig.Calculate(SubTotal);
        public decimal GrandTotal    => SubTotal + Shipping;
        public int     ItemCount     => Items.Sum(i => i.Quantity);

        /// <summary>Amount still needed to qualify for free shipping (0 if already free).</summary>
        public decimal NeededForFree => Shipping > 0
                                        ? ShippingConfig.FreeThreshold - SubTotal
                                        : 0m;
        public decimal FreeThreshold => ShippingConfig.FreeThreshold;
    }
}
