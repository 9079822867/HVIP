using System;
using System.Web.Mvc;
using HVIP.Helpers;
using HVIP.Models;
using Newtonsoft.Json;

namespace HVIP.Controllers
{
    public class CheckoutController : BaseController
    {
        public ActionResult Index()
        {
            var cart = CartHelper.GetCart(Session);
            if (cart.ItemCount == 0) return RedirectToAction("Index", "Cart");
            return View(new CheckoutViewModel { Cart = cart });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(CheckoutViewModel vm)
        {
            var cart = CartHelper.GetCart(Session);
            vm.Cart = cart;

            if (!ModelState.IsValid)
                return View(vm);

            var order = new Order
            {
                OrderNumber = "HVIP" + DateTime.Now.ToString("yyyyMMddHHmm") + new Random().Next(10, 99),
                Items = cart.Items,
                CustomerName = vm.FullName,
                Email = vm.Email,
                Phone = vm.Phone,
                Address = vm.Address,
                City = vm.City,
                State = vm.State,
                Pincode = vm.Pincode,
                PaymentMethod = vm.PaymentMethod ?? "COD",
                SubTotal = cart.SubTotal,
                Shipping = cart.Shipping,
                GrandTotal = cart.GrandTotal,
                OrderDate = DateTime.Now,
                Status = "Confirmed"
            };

            // Save order to database (userId = null for guest checkout)
            int? userId = AuthHelper.IsLoggedIn(Session)
                          ? (int?)AuthHelper.GetUserId(Session)
                          : null;
            OrderRepository.SaveOrder(order, userId);

            TempData["Order"] = JsonConvert.SerializeObject(order);
            CartHelper.ClearCart(Session);
            return RedirectToAction("Success");
        }

        public ActionResult Success()
        {
            var json = TempData["Order"] as string;
            if (string.IsNullOrEmpty(json)) return RedirectToAction("Index", "Home");
            ViewBag.CartCount = 0;
            return View(JsonConvert.DeserializeObject<Order>(json));
        }
    }
}
