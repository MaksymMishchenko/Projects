using Microsoft.AspNetCore.Mvc;
using SportsStore.Domain.Entities;
using SportsStore.Domain.Interfaces;
using SportsStore.WebUI.Binders;
using SportsStore.WebUI.Extensions;
using SportsStore.WebUI.Models;

namespace SportsStore.WebUI.Controllers
{
    public class CartController : Controller
    {
        private readonly IProductRepository _repository;
        public CartController(IProductRepository repo)
        {
            _repository = repo;
        }

        public IActionResult Index([ModelBinder(typeof(CartModelBinder))] Cart cart, string returnUrl)
        {
            var model = new CartIndexViewModel { Cart = cart, ReturnUrl = returnUrl };
            return View(model);
        }

        public IActionResult AddToCart([ModelBinder(typeof(CartModelBinder))] Cart cart, int productId, string returnUrl)
        {
            var product = _repository.Products.FirstOrDefault(p => p.ProductId == productId);

            if (product != null)
            {
                cart.AddItem(product, 1);
                HttpContext.Session.Set("Cart", cart);
            }

            return RedirectToAction("Index", new { returnUrl });
        }

        public IActionResult RemoveFromCart(Cart cart, int productId, string returnUrl)
        {
            var product = _repository.Products.FirstOrDefault(p => p.ProductId == productId);

            if (product != null)
            {
                cart.RemoveLine(product);
            }

            return RedirectToAction("Index", new { returnUrl });
        }
    }
}
