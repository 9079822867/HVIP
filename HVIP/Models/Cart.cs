using System.Collections.Generic;

namespace HVIP.Models
{
    public class CartItem
    {
        public Product Product { get; set; }
        public int Quantity { get; set; }
    }

    public class Cart
    {
        public List<CartItem> Items { get; set; } = new List<CartItem>();
    }
}