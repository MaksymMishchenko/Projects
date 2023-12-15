using Microsoft.AspNetCore.Mvc;
using SportsStore.Domain.Interfaces;

namespace SportsStore.WebUI.Components
{
    public class NavPartialViewComponent : ViewComponent
    {
        private readonly IProductRepository _repository;
        public NavPartialViewComponent(IProductRepository repo)
        {
            _repository = repo;
        }
        public IViewComponentResult Invoke()
        {
            //ViewBag.SelectedCategory = category;
            var categories = _repository.Products.Select(p => p.Category).Distinct().OrderBy(c => c);
            return View("_Menu", categories);
        }
    }
}
