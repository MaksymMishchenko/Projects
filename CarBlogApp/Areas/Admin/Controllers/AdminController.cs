using CarBlogApp.Areas.Admin.Models;
using CarBlogApp.Interfaces;
using CarBlogApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CarBlogApp.Controllers
{
    [Area("Admin")]
    public class AdminController : Controller
    {
        private readonly ILogger<AdminController> _logger;
        private readonly IMessageService _msgService;
        private readonly IPostService _postService;
        private readonly ICategoryService _categoryService;

        public AdminController(ILogger<AdminController> logger,
            IMessageService msgService,
            IPostService postService,
            ICategoryService categoryService)
        {
            _logger = logger;
            _msgService = msgService;
            _postService = postService;
            _categoryService = categoryService;
        }

        public async Task<IActionResult> Index()
        {
            var posts = await _postService.GetAllPostsAsync();

            if (posts == null)
            {
                return NotFound();
            }

            return View(posts);
        }

        public async Task<IActionResult> ShowAllCategories()
        {
            try
            {
                var categories = await _categoryService.GetAllCategoriesAsync();
                return View(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, "Internal server error");

            }
        }

        public IActionResult AddCategory()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddCategory(Category category)
        {
            if (ModelState.IsValid)
            {
                ViewBag.IsAdded = await _categoryService.CreateNewCategoryAsync(category);
                try
                {
                    return View("ShowAllCategories", await _categoryService.GetAllCategoriesAsync());
                }
                catch (Exception ex)
                {

                    _logger.LogError(ex.Message);
                    return StatusCode(500, "Internal Server Error");
                }
            }

            return View(category);
        }

        public async Task<IActionResult> EditCategory(int? id)
        {
            var currentCategory = await _categoryService.FindCategoryAsync(id);

            if (currentCategory == null)
            {
                return NotFound();
            }

            return View(currentCategory);
        }

        [HttpPost]
        public async Task<IActionResult> EditCategory(Category category)
        {
            if (ModelState.IsValid)
            {
                ViewBag.IsEdited = await _categoryService.UpdateCurrentCategoryAsync(category);

                try
                {
                    return View("ShowAllCategories", await _categoryService.GetAllCategoriesAsync());
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                }
            }

            return View(category);
        }

        public async Task<IActionResult> OnDeleteCategory(int? id)
        {
            ViewBag.IsDeleted = await _categoryService.DeleteCategory(id);

            try
            {
                return View("ShowAllCategories", await _categoryService.GetAllCategoriesAsync());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }

        public async Task<IActionResult> AddPost()
        {
            try
            {
                var categorySelectList = new SelectList(await _categoryService.GetAllCategoriesAsync(), "Id", "Name");
                var model = new CreatePostViewModel
                {
                    Post = new Post() { Date = DateTime.Today },
                    Categories = categorySelectList
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddPost(CreatePostViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                ViewBag.IsPostAdded = await _postService.AddNewPostAsync(viewModel);
                return View("Index", await _postService.GetAllPostsAsync());
            }

            return View(viewModel);
        }

        public async Task<IActionResult> EditPost(int? id)
        {
            var exitingPost = await _postService.FindPostAsync(id);

            if (exitingPost == null)
            {
                return NotFound();
            }

            var model = new CreatePostViewModel
            {
                Post = exitingPost,
                Categories = new SelectList(await _categoryService.GetAllCategoriesAsync(), "Id", "Name")
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditPost(CreatePostViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                ViewBag.IsUpdated = await _postService.UpdatePostAsync(viewModel);

                return View("Index", await _postService.GetAllPostsAsync());
            }

            return View(viewModel);
        }

        public async Task<IActionResult> DeletePost(int? id)
        {
            ViewBag.IsDeleted = await _postService.RemovePostAsync(id);

            return View("Index", await _postService.GetAllPostsAsync());
        }

        public async Task<IActionResult> ShowPostsByCategory(int? id)
        {
            return View("Index", await _postService.GetPostsByCategoryAsync(id));
        }

        public async Task<IActionResult> ShowInboxMessages()
        {
            return View(await _msgService.GetInboxMessagesAsync());
        }

        public async Task<IActionResult> RemoveMessage(int? id)
        {
            ViewBag.IsRemoved = await _msgService.RemoveInboxMessageAsync(id);

            return View("ShowInboxMessages", await _msgService.GetInboxMessagesAsync());
        }
    }
}