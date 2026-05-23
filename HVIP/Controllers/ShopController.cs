using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using HVIP.Helpers;
using HVIP.Models;

namespace HVIP.Controllers
{
    public class ShopController : BaseController
    {
        public ActionResult Index(int? categoryId, string q, string sort, decimal? minPrice, decimal? maxPrice)
        {
            var products = ProductCatalog.GetAll();

            if (categoryId.HasValue)
                products = products.Where(p => p.CategoryId == categoryId.Value).ToList();

            if (!string.IsNullOrWhiteSpace(q))
                products = products.Where(p =>
                    p.Name.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    p.ShortDescription.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0).ToList();

            if (minPrice.HasValue) products = products.Where(p => p.Price >= minPrice.Value).ToList();
            if (maxPrice.HasValue) products = products.Where(p => p.Price <= maxPrice.Value).ToList();

            switch (sort ?? "featured")
            {
                case "price_asc":   products = products.OrderBy(p => p.Price).ToList(); break;
                case "price_desc":  products = products.OrderByDescending(p => p.Price).ToList(); break;
                case "rating":      products = products.OrderByDescending(p => p.Rating).ToList(); break;
                case "name":        products = products.OrderBy(p => p.Name).ToList(); break;
                default:            products = products.OrderByDescending(p => p.IsFeatured).ThenByDescending(p => p.IsBestseller).ToList(); break;
            }

            var vm = new ShopViewModel
            {
                Products = products,
                Categories = ProductCatalog.GetCategories(),
                SearchQuery = q,
                SelectedCategoryId = categoryId,
                SortBy = sort,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                TotalProducts = products.Count
            };

            return View(vm);
        }

        public ActionResult Details(int id)
        {
            var product = ProductCatalog.GetById(id);
            if (product == null) return HttpNotFound();
            ViewBag.RelatedProducts = ProductCatalog.GetRelated(id, product.CategoryId, 4);
            return View(product);
        }
    }
}
