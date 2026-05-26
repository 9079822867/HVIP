using System.Web.Mvc;
using HVIP.Helpers;
using HVIP.Models;

namespace HVIP.Controllers
{
    public class AccountController : BaseController
    {
        // GET: /Account/Login
        [HttpGet]
        public ActionResult Login(string returnUrl)
        {
            if (AuthHelper.IsLoggedIn(Session))
                return RedirectToAction("Index", "Home");

            var model = new LoginViewModel { ReturnUrl = returnUrl };
            return View(model);
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = UserRepository.ValidateLogin(model.Email, model.Password);
            if (user == null)
            {
                ModelState.AddModelError("", "Invalid email or password. Please try again.");
                return View(model);
            }

            AuthHelper.SignIn(Session, user.Id, user.Name, user.Email, user.IsAdmin);
            TempData["Welcome"] = "Welcome back, " + user.Name + "! 👋";

            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                return Redirect(model.ReturnUrl);

            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/Register
        [HttpGet]
        public ActionResult Register()
        {
            if (AuthHelper.IsLoggedIn(Session))
                return RedirectToAction("Index", "Home");

            return View(new RegisterViewModel());
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (UserRepository.EmailExists(model.Email))
            {
                ModelState.AddModelError("Email", "This email is already registered. Please log in.");
                return View(model);
            }

            var user = UserRepository.Register(model.Name, model.Email, model.Phone, model.Password);
            if (user == null)
            {
                ModelState.AddModelError("", "Registration failed. Please try again.");
                return View(model);
            }

            AuthHelper.SignIn(Session, user.Id, user.Name, user.Email, user.IsAdmin);
            TempData["Welcome"] = "Account created! Welcome to HVIP, " + user.Name + "! 🌿";
            return RedirectToAction("Index", "Home");
        }

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Logout()
        {
            AuthHelper.SignOut(Session);
            TempData["LogoutMsg"] = "You have been logged out successfully.";
            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/Profile
        [HttpGet]
        public ActionResult Profile()
        {
            if (!AuthHelper.IsLoggedIn(Session))
                return RedirectToAction("Login", new { returnUrl = "/Account/Profile" });

            int userId = AuthHelper.GetUserId(Session);
            var user   = UserRepository.GetById(userId);
            if (user == null) return RedirectToAction("Login");

            ViewBag.UserOrders = UserRepository.GetUserOrders(userId);
            return View(user);
        }

        // POST: /Account/Profile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Profile(User model)
        {
            if (!AuthHelper.IsLoggedIn(Session))
                return RedirectToAction("Login");

            var userId = AuthHelper.GetUserId(Session);

            // Only validate the fields we care about
            ModelState.Remove("PasswordHash");
            ModelState.Remove("Email");
            ModelState.Remove("RegisteredOn");

            if (!ModelState.IsValid)
            {
                var existing = UserRepository.GetById(userId);
                if (existing != null)
                {
                    model.Email        = existing.Email;
                    model.RegisteredOn = existing.RegisteredOn;
                }
                return View(model);
            }

            UserRepository.UpdateProfile(userId, model.Name, model.Phone,
                                         model.Address, model.City, model.State, model.Pincode);

            // Update session name if changed
            Session["HVIP_UserName"] = model.Name;
            TempData["ProfileSaved"] = "Profile updated successfully!";
            return RedirectToAction("Profile");
        }

        // ── My Orders ──────────────────────────────────────────

        // GET: /Account/Orders
        public ActionResult Orders()
        {
            if (!AuthHelper.IsLoggedIn(Session))
                return RedirectToAction("Login", new { returnUrl = "/Account/Orders" });

            int userId = AuthHelper.GetUserId(Session);
            ViewBag.Title = "My Orders";
            return View(UserRepository.GetUserOrders(userId));
        }

        // GET: /Account/OrderDetail/5
        public ActionResult OrderDetail(int id)
        {
            if (!AuthHelper.IsLoggedIn(Session))
                return RedirectToAction("Login");

            int userId = AuthHelper.GetUserId(Session);
            var order  = OrderRepository.GetDetail(id, userId);
            if (order == null) return HttpNotFound();

            ViewBag.Title = "Order #" + order.OrderNumber;
            return View(order);
        }

        // ── Change Password ────────────────────────────────────

        // GET: /Account/ChangePassword
        [HttpGet]
        public ActionResult ChangePassword()
        {
            if (!AuthHelper.IsLoggedIn(Session))
                return RedirectToAction("Login", new { returnUrl = "/Account/ChangePassword" });

            ViewBag.Title = "Change Password";
            return View(new ChangePasswordViewModel());
        }

        // POST: /Account/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(ChangePasswordViewModel model)
        {
            if (!AuthHelper.IsLoggedIn(Session))
                return RedirectToAction("Login");

            ViewBag.Title = "Change Password";

            if (!ModelState.IsValid)
                return View(model);

            int userId = AuthHelper.GetUserId(Session);
            var user   = UserRepository.GetById(userId);

            if (user == null || !AuthHelper.VerifyPassword(model.CurrentPassword, user.PasswordHash))
            {
                ModelState.AddModelError("CurrentPassword", "Current password is incorrect.");
                return View(model);
            }

            UserRepository.ChangePassword(userId, model.NewPassword);
            TempData["ProfileSaved"] = "Password changed successfully!";
            return RedirectToAction("Profile");
        }
    }
}
