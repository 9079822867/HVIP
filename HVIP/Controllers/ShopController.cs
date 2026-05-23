using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using HVIP.Models;

namespace HVIP.Controllers
{
    public class ShopController : Controller
    {
        // In-memory product list for demo
        private static List<Product> Products = new List<Product>
        {
            new Product { Id = 1, Name = "Herbal Oil", Description = "Ayurvedic herbal oil.", Price = 15.99m, ImageUrl = "/Content/Images/oil.jpg" },
            new Product { Id = 2, Name = "Chyawanprash", Description = "Immunity booster.", Price = 12.50m, ImageUrl = "/Content/Images/chyawanprash.jpg" }
        };

        public ActionResult Index()
        {
            return View(Products);
        }

        public ActionResult Details(int id)
        {
            var product = Products.FirstOrDefault(p => p.Id == id);
            if (product == null) return HttpNotFound();
            return View(product);
        }
    }
}
