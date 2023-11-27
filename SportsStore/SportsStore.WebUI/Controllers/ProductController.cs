using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportsStore.Domain.Interfaces;
using SportsStore.WebUI.Models;
using System.Diagnostics;

namespace SportsStore.WebUI.Controllers
{
    public class ProductController : Controller
    {
        private readonly ILogger<ProductController> _logger;
        private readonly IProductRepository _repository;

        public ProductController(ILogger<ProductController> logger, IProductRepository repository)
        {
            _logger = logger;
            _repository = repository;   
        }

        public async Task<IActionResult> List()
        {
            return View(await _repository.Products.ToArrayAsync());
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
