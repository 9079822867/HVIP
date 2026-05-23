using System;

namespace HVIP.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ShortDescription { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public decimal? OriginalPrice { get; set; }
        public string ImageUrl { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string CategoryIcon { get; set; }
        public string CategoryColor { get; set; }
        public string Brand { get; set; }
        public int Stock { get; set; }
        public bool IsFeatured { get; set; }
        public bool IsBestseller { get; set; }
        public bool IsNew { get; set; }
        public double Rating { get; set; }
        public int ReviewCount { get; set; }
        public string Size { get; set; }

        public int DiscountPercent
        {
            get
            {
                if (OriginalPrice.HasValue && OriginalPrice.Value > 0)
                    return (int)Math.Round((1m - Price / OriginalPrice.Value) * 100);
                return 0;
            }
        }

        public bool IsOnSale => DiscountPercent > 0;
    }
}
