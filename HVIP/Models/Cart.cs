using System.Collections.Generic;
using System.Linq;

namespace HVIP.Models
{
    public class CartItem
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductSize { get; set; }
        public string CategoryIcon { get; set; }
        public string CategoryColor { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal Total => UnitPrice * Quantity;
    }

    public class Cart
    {
        public List<CartItem> Items { get; set; } = new List<CartItem>();

        public decimal SubTotal => Items.Sum(i => i.Total);
        public decimal Shipping => SubTotal >= 500 ? 0 : 60;
        public decimal GrandTotal => SubTotal + Shipping;
        public int ItemCount => Items.Sum(i => i.Quantity);
    }
}
