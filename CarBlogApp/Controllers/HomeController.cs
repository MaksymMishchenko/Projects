using CarBlogApp.Interfaces;
using CarBlogApp.Models;
using CarBlogApp.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace CarBlogApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IPostService _postService;
        private readonly ICategoryService _categoryService;
        private readonly IMessageService _messageService;

        public HomeController(ILogger<HomeController> logger,
            IPostService postService,
            ICategoryService categoryService,
            IMessageService messageService)
        {
            _logger = logger;
            _postService = postService;
            _categoryService = categoryService;
            _messageService = messageService;
        }

        public async Task<IActionResult> Index()
        {
            var categoryModel = new CategoriesViewModel
            {
                Categories = await _categoryService.GetAllCategoriesAsync()
            };

            ViewData["CategoriesViewModel"] = categoryModel;

            var posts = await _postService.GetAllPostsAsync();

            return View(posts);
        }

        public async Task<IActionResult> ShowPostsByCategoryId(int? id)
        {
            var categoryModel = new CategoriesViewModel
            {
                Categories = await _categoryService.GetAllCategoriesAsync()
            };

            ViewData["CategoriesViewModel"] = categoryModel;

            var posts = await _postService.GetPostsByCategoryAsync(id);

            if (posts == null)
            {
                return NotFound();
            }

            return View(posts);
        }

        public async Task<IActionResult> ShowFullPost(int? id)
        {
            var categoryModel = new CategoriesViewModel
            {
                Categories = await _categoryService.GetAllCategoriesAsync()
            };

            ViewData["CategoriesViewModel"] = categoryModel;

            var post = await _postService.FindPostAsync(id);

            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }

        public async Task<IActionResult> About()
        {
            var categoryModel = new CategoriesViewModel
            {
                Categories = await _categoryService.GetAllCategoriesAsync()
            };

            ViewData["CategoriesViewModel"] = categoryModel;

            return View();
        }
        public async Task<IActionResult> Contact()
        {
            var categoryModel = new CategoriesViewModel
            {
                Categories = await _categoryService.GetAllCategoriesAsync()
            };

            ViewData["CategoriesViewModel"] = categoryModel;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Contact(ContactForm messages)
        {
            var categoryModel = new CategoriesViewModel
            {
                Categories = await _categoryService.GetAllCategoriesAsync()
            };

            ViewData["CategoriesViewModel"] = categoryModel;

            if (ModelState.IsValid)
            {
                ViewBag.IsSent = await _messageService.AddMessageToInbox(messages);

                return View("FormSentSuccess");
            }

            return View(messages);
        }

        public async Task<IActionResult> GetMagazine()
        {
            string path = "wwwroot\\files";
            string file = "sample.pdf";
            string contentType = "application/octet-stream";

            var memory = await DownloadFile(path, file);

            return File(memory, contentType, file);
        }

        private static async Task<MemoryStream> DownloadFile(string uploadPath, string fileName)
        {
            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), uploadPath, fileName);
            var memory = new MemoryStream();
            if (System.IO.File.Exists(fullPath))
            {
                using (var stream = new FileStream(fullPath, FileMode.Open))
                {
                    await stream.CopyToAsync(memory);
                }

                memory.Position = 0;
            }

            return memory;
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}