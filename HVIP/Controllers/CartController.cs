using System.Web.Mvc;
using HVIP.Helpers;

namespace HVIP.Controllers
{
    public class CartController : BaseController
    {
        public ActionResult Index()
        {
            var cart = CartHelper.GetCart(Session);
            return View(cart);
        }

        [HttpPost]
        public ActionResult Add(int id, int qty = 1)
        {
            var product = ProductCatalog.GetById(id);
            if (product != null)
                CartHelper.AddItem(Session, product, qty);

            if (Request.IsAjaxRequest())
                return Json(new { success = true, count = CartHelper.GetCart(Session).ItemCount });

            return RedirectToAction("Index");
        }

        public ActionResult Remove(int id)
        {
            CartHelper.RemoveItem(Session, id);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult UpdateQty(int id, int qty)
        {
            CartHelper.UpdateQuantity(Session, id, qty);
            return RedirectToAction("Index");
        }

        public ActionResult Count()
        {
            return Json(CartHelper.GetCart(Session).ItemCount, JsonRequestBehavior.AllowGet);
        }
    }
}
