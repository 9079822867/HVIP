using System;
using System.Collections.Generic;

namespace HVIP.Models
{
    public class Order
    {
        public int Id { get; set; }
        public List<CartItem> Items { get; set; }
        public string CustomerName { get; set; }
        public string Address { get; set; }
        public DateTime OrderDate { get; set; }
    }
}