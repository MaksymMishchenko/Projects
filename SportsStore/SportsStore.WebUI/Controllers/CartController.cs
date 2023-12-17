using Microsoft.AspNetCore.Mvc;
using SportsStore.Domain.Entities;
using SportsStore.Domain.Interfaces;
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

        public IActionResult Index(string returnUrl)
        {
            var model = new CartIndexViewModel { Cart = GetCart(), ReturnUrl = returnUrl };
            return View(model);
        }

        public IActionResult AddToCart(int productId, string returnUrl)
        {
            var product = _repository.Products.FirstOrDefault(p => p.ProductId == productId);

            if (product != null)
            {
                var getCart = HttpContext.Session.Get<Cart>("Cart");

                if (getCart == null)
                {
                    var newCart = new Cart();
                    newCart.AddItem(product, 1);
                    HttpContext.Session.Set("Cart", newCart);
                }
                else
                {
                    Cart? existCart = getCart;
                    existCart?.AddItem(product, 1);
                    HttpContext.Session.Set("Cart", existCart);
                }
            }

            return RedirectToAction("Index", new { returnUrl });
        }

        public IActionResult RemoveFromCart(int productId, string returnUrl)
        {
            var product = _repository.Products.FirstOrDefault(p => p.ProductId == productId);
        
            if (product != null)
            {
                GetCart()?.RemoveLine(product);
            }
        
            return RedirectToAction("Index", new { returnUrl });
        }

        private Cart? GetCart()
        {
            return HttpContext.Session.Get<Cart>("Cart");
        }
    }
}
