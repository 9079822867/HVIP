using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using HVIP.Helpers;
using HVIP.Models;

namespace HVIP.Controllers
{
    public class AdminController : BaseController
    {
        // ── Admin guard ───────────────────────────────────────
        protected override void OnActionExecuting(ActionExecutingContext ctx)
        {
            base.OnActionExecuting(ctx);
            if (!AuthHelper.IsLoggedIn(Session) || !AuthHelper.IsAdmin(Session))
                ctx.Result = RedirectToAction("Login", "Account",
                                              new { returnUrl = Request.Url.AbsoluteUri });
        }

        // ── Dashboard ─────────────────────────────────────────
        public ActionResult Index()
        {
            ViewBag.Title = "Dashboard";
            return View(AdminRepository.GetDashboard());
        }

        // ══ PRODUCTS ══════════════════════════════════════════

        public ActionResult Products()
        {
            ViewBag.Title = "Products";
            return View(ProductCatalog.GetAllAdmin());
        }

        [HttpGet]
        public ActionResult ProductAdd()
        {
            ViewBag.Title = "Add Product";
            return View("ProductForm", BuildProductForm(new ProductFormViewModel()));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult ProductAdd(ProductFormViewModel f)
        {
            if (!ModelState.IsValid) return View("ProductForm", BuildProductForm(f));
            AdminRepository.SaveProduct(f);
            TempData["AdminMsg"] = "Product added successfully!";
            return RedirectToAction("Products");
        }

        [HttpGet]
        public ActionResult ProductEdit(int id)
        {
            var p = ProductCatalog.GetByIdAdmin(id);
            if (p == null) return HttpNotFound();
            ViewBag.Title = "Edit Product";
            var f = new ProductFormViewModel
            {
                Id = p.Id, Name = p.Name, ShortDescription = p.ShortDescription,
                Description = p.Description, Price = p.Price, OriginalPrice = p.OriginalPrice,
                CategoryId = p.CategoryId, Brand = p.Brand, Stock = p.Stock, Size = p.Size,
                IsFeatured = p.IsFeatured, IsBestseller = p.IsBestseller, IsNew = p.IsNew,
                IsActive = p.IsActive, Rating = p.Rating, ReviewCount = p.ReviewCount
            };
            return View("ProductForm", BuildProductForm(f));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult ProductEdit(ProductFormViewModel f)
        {
            if (!ModelState.IsValid) return View("ProductForm", BuildProductForm(f));
            AdminRepository.SaveProduct(f);
            TempData["AdminMsg"] = "Product updated successfully!";
            return RedirectToAction("Products");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult ProductDelete(int id)
        {
            AdminRepository.DeleteProduct(id);
            TempData["AdminMsg"] = "Product deleted.";
            return RedirectToAction("Products");
        }

        [HttpPost]
        public JsonResult ProductToggle(int id, string field)
        {
            bool ok = AdminRepository.ToggleProduct(id, field);
            return Json(new { success = ok });
        }

        private ProductFormViewModel BuildProductForm(ProductFormViewModel f)
        {
            f.CategoryOptions = ProductCatalog.GetCategories()
                .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name,
                                                  Selected = c.Id == f.CategoryId }).ToList();
            return f;
        }

        // ══ CATEGORIES ════════════════════════════════════════

        public ActionResult Categories()
        {
            ViewBag.Title = "Categories";
            return View(ProductCatalog.GetCategoriesAdmin());
        }

        [HttpGet]
        public ActionResult CategoryAdd()
        {
            ViewBag.Title = "Add Category";
            return View("CategoryForm", new Category());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult CategoryAdd(Category c)
        {
            if (!ModelState.IsValid) return View("CategoryForm", c);
            AdminRepository.SaveCategory(c);
            TempData["AdminMsg"] = "Category added!";
            return RedirectToAction("Categories");
        }

        [HttpGet]
        public ActionResult CategoryEdit(int id)
        {
            var c = ProductCatalog.GetCategory(id);
            if (c == null) return HttpNotFound();
            ViewBag.Title = "Edit Category";
            return View("CategoryForm", c);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult CategoryEdit(Category c)
        {
            if (!ModelState.IsValid) return View("CategoryForm", c);
            AdminRepository.SaveCategory(c);
            TempData["AdminMsg"] = "Category updated!";
            return RedirectToAction("Categories");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult CategoryDelete(int id)
        {
            AdminRepository.DeleteCategory(id);
            TempData["AdminMsg"] = "Category deleted.";
            return RedirectToAction("Categories");
        }

        // ══ BANNERS ═══════════════════════════════════════════

        public ActionResult Banners()
        {
            ViewBag.Title = "Banners";
            return View(BannerRepository.GetAll());
        }

        [HttpGet]
        public ActionResult BannerAdd()
        {
            ViewBag.Title = "Add Banner";
            return View("BannerForm", new Banner { IsActive = true, SortOrder = 1,
                Icon = "fas fa-leaf", BgGradient = "linear-gradient(135deg,#0d4a1e,#1a6e2e)" });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult BannerAdd(Banner b)
        {
            BannerRepository.Save(b);
            TempData["AdminMsg"] = "Banner added!";
            return RedirectToAction("Banners");
        }

        [HttpGet]
        public ActionResult BannerEdit(int id)
        {
            var b = BannerRepository.GetById(id);
            if (b == null) return HttpNotFound();
            ViewBag.Title = "Edit Banner";
            return View("BannerForm", b);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult BannerEdit(Banner b)
        {
            BannerRepository.Save(b);
            TempData["AdminMsg"] = "Banner updated!";
            return RedirectToAction("Banners");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult BannerDelete(int id)
        {
            BannerRepository.Delete(id);
            TempData["AdminMsg"] = "Banner deleted.";
            return RedirectToAction("Banners");
        }

        // ══ ORDERS ════════════════════════════════════════════

        public ActionResult Orders()
        {
            ViewBag.Title = "Orders";
            return View(AdminRepository.GetOrders());
        }

        [HttpPost]
        public ActionResult OrderStatus(int id, string status)
        {
            AdminRepository.UpdateOrderStatus(id, status);
            TempData["AdminMsg"] = "Order status updated.";
            return RedirectToAction("Orders");
        }

        // ══ MESSAGES ══════════════════════════════════════════

        public ActionResult Messages()
        {
            ViewBag.Title = "Contact Messages";
            return View(AdminRepository.GetMessages());
        }
    }
}
