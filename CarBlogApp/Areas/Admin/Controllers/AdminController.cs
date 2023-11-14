using CarBlogApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarBlogApp.Controllers
{
    [Area("Admin")]
    public class AdminController : Controller
    {
        public async Task<IActionResult> Index()
        {
            var posts = await GetAllPosts();

            return View(posts);
        }

        private async Task<IEnumerable<Post>> GetAllPosts()
        {
            IEnumerable<Post> posts = new List<Post>();

            using (var db = new DatabaseContext())
            {
                posts = await db.Posts.Include(p => p.Category).ToListAsync();
            }

            return posts;
        }

        public async Task<IActionResult> ShowAllCategories()
        {
            var categories = await GetAllCategories();

            if (categories == null)
            {
                return NotFound();
            }

            return View(categories);
        }

        private async Task<IEnumerable<Category>> GetAllCategories()
        {
            IEnumerable<Category> categories = new List<Category>();

            using (var db = new DatabaseContext())
            {
                categories = await db.Categories.ToListAsync();
            }

            return categories;
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
                try
                {
                    await AddNewCategory(category);
                }

                catch (Exception ex)
                {
                    return View(ex.Message, category);
                }
            }

            return View("Success");
        }
        /// <summary>
        /// Add new category from category form to database asynchronously or trow InvalidOperationException if category exists
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        private async Task AddNewCategory(Category category)
        {
            using (var db = new DatabaseContext())
            {
                var categoryExist = await db.Categories.FirstOrDefaultAsync(c => c.Name == category.Name);

                if (categoryExist == null)
                {
                    await db.Categories.AddAsync(new Category { Name = category.Name?.Trim() });
                    await db.SaveChangesAsync();
                }
                else
                {
                    throw new InvalidOperationException("SuchCategoryExist");
                }
            }
        }
    }
}