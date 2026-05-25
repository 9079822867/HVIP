using System.Web.Mvc;
using HVIP.Helpers;
using HVIP.Models;

namespace HVIP.Controllers
{
    public class ShopController : BaseController
    {
        public ActionResult Index(int? categoryId, string q, string sort, decimal? minPrice, decimal? maxPrice)
        {
            // All filtering & sorting done inside ProductCatalog.Search() via SQL
            var products = ProductCatalog.Search(
                keyword:    q,
                categoryId: categoryId,
                minPrice:   minPrice,
                maxPrice:   maxPrice,
                sortBy:     sort
            );

            var vm = new ShopViewModel
            {
                Products           = products,
                Categories         = ProductCatalog.GetCategories(),
                SearchQuery        = q,
                SelectedCategoryId = categoryId,
                SortBy             = sort,
                MinPrice           = minPrice,
                MaxPrice           = maxPrice,
                TotalProducts      = products.Count
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
