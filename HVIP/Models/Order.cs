using System;
using System.Collections.Generic;

namespace HVIP.Models
{
    public class Order
    {
        public int    Id          { get; set; }
        public string OrderNumber { get; set; }
        public List<CartItem> Items { get; set; }
        public string CustomerName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Pincode { get; set; }
        public string PaymentMethod { get; set; }
        public decimal SubTotal { get; set; }
        public decimal Shipping { get; set; }
        public decimal GrandTotal { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; }
    }
}
