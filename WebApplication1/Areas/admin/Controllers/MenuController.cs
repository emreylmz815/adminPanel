using System;
using System.Linq;
using System.Web.Mvc;
using WebApplication1.Models;
using WebApplication1.Services;

namespace WebApplication1.Areas.admin.Controllers
{
    public class MenuController : Controller
    {
        private readonly CafeMenuService _menuService;

        public MenuController()
        {
            _menuService = new CafeMenuService();
        }

        [HttpGet]
        public ActionResult Index(Guid? categoryId)
        {
            var menu = _menuService.GetOrCreateMenu(HttpContext);

            var orderedCategories = menu.Categories
                .OrderBy(category => category.Name, StringComparer.CurrentCultureIgnoreCase)
                .ToList();

            Guid activeCategoryId;
            if (categoryId.HasValue && menu.FindCategory(categoryId.Value) != null)
            {
                activeCategoryId = categoryId.Value;
            }
            else
            {
                activeCategoryId = orderedCategories.Any() ? orderedCategories.First().Id : Guid.Empty;
            }

            var viewModel = new CafeMenuViewModel
            {
                Menu = menu,
                ActiveCategoryId = activeCategoryId,
                ProductForm = new MenuProductInput { CategoryId = activeCategoryId }
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddCategory(string name)
        {
            var menu = _menuService.GetOrCreateMenu(HttpContext);
            _menuService.AddCategory(menu, name);
            _menuService.SaveMenu(HttpContext, menu);
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RemoveCategory(Guid categoryId)
        {
            var menu = _menuService.GetOrCreateMenu(HttpContext);
            _menuService.RemoveCategory(menu, categoryId);
            _menuService.SaveMenu(HttpContext, menu);
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddProduct(MenuProductInput input)
        {
            if (input == null)
            {
                return RedirectToAction("Index");
            }

            var menu = _menuService.GetOrCreateMenu(HttpContext);
            _menuService.AddProduct(menu, input);
            _menuService.SaveMenu(HttpContext, menu);
            return RedirectToAction("Index", new { categoryId = input.CategoryId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RemoveProduct(Guid productId, Guid categoryId)
        {
            var menu = _menuService.GetOrCreateMenu(HttpContext);
            _menuService.RemoveProduct(menu, productId);
            _menuService.SaveMenu(HttpContext, menu);
            return RedirectToAction("Index", new { categoryId });
        }
    }
}
