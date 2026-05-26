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

            ViewBag.SeoTitle    = "HVIP Homeopathic – India's Trusted Homeopathic Medicine Store";
            ViewBag.SeoDesc     = "Shop 30+ authentic GMP-certified homeopathic medicines online. Dilutions, mother tinctures, biochemic tablets, combination remedies. Free delivery above ₹500.";
            ViewBag.SeoKeywords = "homeopathic medicines online, buy homeopathy India, HVIP, dilutions, mother tinctures, biochemic tablets, combination remedies, homeopathic store";
            ViewBag.OgType      = "website";
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Title       = "About Us";
            ViewBag.SeoTitle    = "About HVIP Homeopathic – Trusted Since 1984";
            ViewBag.SeoDesc     = "HVIP Homeopathic is India's leading GMP-certified homeopathic medicine brand trusted by over 10 lakh customers since 1984. ISO 9001:2015 compliant.";
            ViewBag.SeoKeywords = "about HVIP homeopathic, GMP certified homeopathy, ISO 9001 homeopathic brand India";
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Title       = "Contact Us";
            ViewBag.SeoTitle    = "Contact HVIP Homeopathic – Customer Support";
            ViewBag.SeoDesc     = "Get in touch with HVIP Homeopathic. Call 1800-123-4567 or email info@hvip.com. We're available Monday to Saturday, 9am–6pm.";
            ViewBag.SeoKeywords = "contact HVIP homeopathic, HVIP customer support, homeopathic medicine helpline";
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
