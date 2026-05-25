using System.Collections.Generic;

namespace HVIP.Models
{
    public class OrderSummary
    {
        public int      Id            { get; set; }
        public string   OrderNumber   { get; set; }
        public string   CustomerName  { get; set; }
        public string   Email         { get; set; }
        public decimal  GrandTotal    { get; set; }
        public string   PaymentMethod { get; set; }
        public string   Status        { get; set; }
        public System.DateTime OrderDate { get; set; }
        public int      ItemCount     { get; set; }
    }

    public class AdminDashboardViewModel
    {
        public int     TotalProducts    { get; set; }
        public int     ActiveProducts   { get; set; }
        public int     TotalCategories  { get; set; }
        public int     TotalBanners     { get; set; }
        public int     TotalOrders      { get; set; }
        public decimal TotalRevenue     { get; set; }
        public int     TotalUsers       { get; set; }
        public int     TotalMessages    { get; set; }
        public int     UnreadMessages   { get; set; }
        public List<OrderSummary> RecentOrders { get; set; }
    }
}
