using System;
using System.Text;
using System.Web.Mvc;
using HVIP.Helpers;

namespace HVIP.Controllers
{
    /// <summary>
    /// Serves /sitemap.xml and /robots.txt — does NOT inherit BaseController
    /// to avoid the overhead of loading categories / cart / session on every crawl.
    /// </summary>
    public class SeoController : Controller
    {
        // ── sitemap.xml ───────────────────────────────────────
        public ActionResult Sitemap()
        {
            string baseUrl = Request.Url.GetLeftPart(UriPartial.Authority);
            var sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            sb.AppendLine("<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">");

            // Static pages
            AddUrl(sb, baseUrl + "/",               "1.0", "daily");
            AddUrl(sb, baseUrl + "/Shop",           "0.9", "daily");
            AddUrl(sb, baseUrl + "/Home/About",     "0.5", "monthly");
            AddUrl(sb, baseUrl + "/Home/Contact",   "0.5", "monthly");

            // Category pages
            try
            {
                foreach (var cat in ProductCatalog.GetCategories())
                    AddUrl(sb, baseUrl + "/Shop?categoryId=" + cat.Id, "0.8", "weekly");
            }
            catch { }

            // Product detail pages
            try
            {
                foreach (var p in ProductCatalog.GetAll())
                    AddUrl(sb, baseUrl + "/Shop/Details/" + p.Id, "0.7", "weekly");
            }
            catch { }

            sb.AppendLine("</urlset>");
            return Content(sb.ToString(), "application/xml", Encoding.UTF8);
        }

        private static void AddUrl(StringBuilder sb, string loc, string priority, string changefreq)
        {
            sb.AppendLine("  <url>");
            sb.AppendLine("    <loc>" + loc + "</loc>");
            sb.AppendLine("    <lastmod>" + DateTime.UtcNow.ToString("yyyy-MM-dd") + "</lastmod>");
            sb.AppendLine("    <changefreq>" + changefreq + "</changefreq>");
            sb.AppendLine("    <priority>" + priority + "</priority>");
            sb.AppendLine("  </url>");
        }

        // ── robots.txt ────────────────────────────────────────
        public ActionResult Robots()
        {
            string baseUrl = Request.Url.GetLeftPart(UriPartial.Authority);
            string content =
$@"User-agent: *
Allow: /
Disallow: /Admin/
Disallow: /Cart/
Disallow: /Checkout/
Disallow: /Account/
Disallow: /Account/Login
Disallow: /Account/Register

Sitemap: {baseUrl}/sitemap.xml
";
            return Content(content, "text/plain", Encoding.UTF8);
        }
    }
}
