using System.Web.Mvc;
using HVIP.Helpers;
using HVIP.Models;

namespace HVIP.Controllers
{
    public class HomeController : BaseController
    {
        public ActionResult Index()
        {
            ViewBag.FeaturedProducts = ProductCatalog.GetFeatured(8);
            ViewBag.BestSellers      = ProductCatalog.GetBestsellers(6);
            ViewBag.NewArrivals      = ProductCatalog.GetNewArrivals(4);
            ViewBag.Banners          = BannerRepository.GetActive();
            return View();
        }

        public ActionResult About()
        {
            return View();
        }

        public ActionResult Contact()
        {
            return View(new ContactMessage());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Contact(ContactMessage msg)
        {
            if (ModelState.IsValid)
            {
                // Save contact message to database
                OrderRepository.SaveContactMessage(msg);

                ViewBag.Success = true;
                ModelState.Clear();
                return View(new ContactMessage());
            }
            return View(msg);
        }
    }
}
