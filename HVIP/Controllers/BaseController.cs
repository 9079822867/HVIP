using System.Web.Mvc;
using HVIP.Helpers;

namespace HVIP.Controllers
{
    public class BaseController : Controller
    {
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            ViewBag.Categories = ProductCatalog.GetCategories();
            ViewBag.CartCount = CartHelper.GetCart(Session).ItemCount;
        }
    }
}
