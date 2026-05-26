using System.Collections.Generic;
using System.Web.Mvc;
using HVIP.Helpers;

namespace HVIP.Controllers
{
    public class BaseController : Controller
    {
        // Controllers whose pages should never appear in search results
        private static readonly HashSet<string> NoIndexControllers =
            new HashSet<string>(System.StringComparer.OrdinalIgnoreCase)
            { "Admin", "Account", "Cart", "Checkout" };

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            ViewBag.Categories      = ProductCatalog.GetCategories();
            ViewBag.CartCount       = CartHelper.GetCart(Session).ItemCount;
            ViewBag.IsLoggedIn      = AuthHelper.IsLoggedIn(Session);
            ViewBag.CurrentUserName = AuthHelper.GetUserName(Session);
            ViewBag.IsAdmin         = AuthHelper.IsAdmin(Session);

            // Auto noindex for private/transactional sections
            string ctrl = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName;
            if (NoIndexControllers.Contains(ctrl))
                ViewBag.SeoNoIndex = true;
        }
    }
}
