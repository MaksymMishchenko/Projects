using Microsoft.AspNetCore.Mvc;
using SportsStore.Domain.Interfaces;

namespace SportsStore.WebUI.Controllers
{
    public class NavController : Controller
    {
        private readonly IProductRepository _repository;
        public NavController(IProductRepository repo)
        {
            _repository = repo;
        }
        public PartialViewResult Menu(string category = null)
        {
            ViewBag.SelectedCategory = category;
            var categories = _repository.Products.Select(p => p.Category).Distinct().OrderBy(c => c); 
            return PartialView("_Menu", categories);
        }
    }
}
