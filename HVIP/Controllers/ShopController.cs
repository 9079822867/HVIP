using System.Web.Mvc;
using HVIP.Helpers;
using HVIP.Models;

namespace HVIP.Controllers
{
    public class ShopController : BaseController
    {
        public ActionResult Index(int? categoryId, string q, string sort, decimal? minPrice, decimal? maxPrice)
        {
            var products = ProductCatalog.Search(
                keyword: q, categoryId: categoryId,
                minPrice: minPrice, maxPrice: maxPrice, sortBy: sort);

            var vm = new ShopViewModel
            {
                Products = products, Categories = ProductCatalog.GetCategories(),
                SearchQuery = q, SelectedCategoryId = categoryId,
                SortBy = sort, MinPrice = minPrice, MaxPrice = maxPrice,
                TotalProducts = products.Count
            };

            // SEO — category page vs search vs general shop
            if (categoryId.HasValue)
            {
                var cat = vm.Categories.Find(c => c.Id == categoryId.Value);
                if (cat != null)
                {
                    ViewBag.SeoTitle    = cat.Name + " – Buy Online | HVIP Homeopathic";
                    ViewBag.SeoDesc     = $"Buy authentic {cat.Name} homeopathic medicines online at HVIP. {(cat.Description ?? "")} GMP certified, fast delivery across India.";
                    ViewBag.SeoKeywords = $"{cat.Name}, buy {cat.Name} online, homeopathic {cat.Name}, HVIP {cat.Name}";
                }
            }
            else if (!string.IsNullOrWhiteSpace(q))
            {
                ViewBag.SeoTitle = $"\"{q}\" – Search Results | HVIP Homeopathic";
                ViewBag.SeoDesc  = $"Search results for \"{q}\" at HVIP Homeopathic. Find authentic homeopathic medicines online with fast delivery.";
                ViewBag.Canonical = null; // no canonical for search pages — prevent indexing
                ViewBag.SeoNoIndex = true;
            }
            else
            {
                ViewBag.SeoTitle    = "Shop Homeopathic Medicines Online | HVIP Homeopathic";
                ViewBag.SeoDesc     = "Browse 30+ authentic homeopathic medicines at HVIP. Dilutions, mother tinctures, biochemic tablets and more. GMP certified. Free delivery above ₹500.";
                ViewBag.SeoKeywords = "shop homeopathic medicines online, buy homeopathy India, HVIP shop";
            }

            return View(vm);
        }

        public ActionResult Details(int id)
        {
            var product = ProductCatalog.GetById(id);
            if (product == null) return HttpNotFound();
            ViewBag.RelatedProducts = ProductCatalog.GetRelated(id, product.CategoryId, 4);

            // Product SEO
            ViewBag.SeoTitle    = product.Name + " – " + product.Brand + " | HVIP Homeopathic";
            ViewBag.SeoDesc     = !string.IsNullOrEmpty(product.ShortDescription)
                                    ? product.ShortDescription + $" Buy {product.Name} online at HVIP. Authentic, GMP certified. Fast delivery."
                                    : $"Buy {product.Name} online at HVIP Homeopathic. Authentic homeopathic medicine. GMP certified, fast delivery across India.";
            ViewBag.SeoKeywords = $"{product.Name}, buy {product.Name} online, {product.Brand}, homeopathic {product.CategoryName}, HVIP";
            ViewBag.OgType      = "product";
            if (!string.IsNullOrEmpty(product.ImageUrl))
                ViewBag.SeoImage = product.ImageUrl;

            return View(product);
        }
    }
}
