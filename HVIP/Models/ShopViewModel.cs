using System.Collections.Generic;

namespace HVIP.Models
{
    public class ShopViewModel
    {
        public List<Product> Products { get; set; }
        public List<Category> Categories { get; set; }
        public string SearchQuery { get; set; }
        public int? SelectedCategoryId { get; set; }
        public string SortBy { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public int TotalProducts { get; set; }
    }
}
