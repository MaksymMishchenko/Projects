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
        public int PageSize = 4;


        public ProductController(ILogger<ProductController> logger, IProductRepository repository)
        {
            _logger = logger;
            _repository = repository;
        }

        public IActionResult List(int page = 1)
        {
            return View(_repository.Products
            .OrderBy(p => p.ProductId)
            .Skip((page - 1) * PageSize)
            .Take(PageSize));
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
