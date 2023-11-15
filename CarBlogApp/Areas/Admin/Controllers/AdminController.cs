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
                posts = await db.Posts.Include(c => c.Category).ToListAsync();
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
                    ViewBag.IsAdded = await AddNewCategory(category);
                }

                catch (Exception ex)
                {
                    return View(ex.Message);
                }
            }

            return View("ShowAllCategories", await GetAllCategories());
        }
        /// <summary>
        /// Add new category from category form to database asynchronously or trow InvalidOperationException if category exists
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        private async Task<bool> AddNewCategory(Category category)
        {
            using (var db = new DatabaseContext())
            {
                //var postsByCategory = await db.Posts.Where(c => c.Category!.Name == category.Name).ToListAsync();

                var categoryExist = await db.Categories.FirstOrDefaultAsync(c => c.Name == category.Name);

                if (categoryExist == null)
                {
                    await db.Categories.AddAsync(new Category { Name = category.Name?.Trim() });
                    await db.SaveChangesAsync();

                    return true;
                }
               
                else
                {
                    return false;
                    throw new InvalidOperationException("Such Category Exist");
                }
            }
        }

        public async Task<IActionResult> EditCategory(int? id)
        {
            var currentCategory = await FindCategoryAsync(id);
            return View(currentCategory);
        }

        [HttpPost]
        public async Task<IActionResult> EditCategory(Category category)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    ViewBag.IsEdited = await UpdateCurrentCategoryAsync(category);

                }

                catch (Exception ex)
                {
                    return View(ex.Message, category);
                }
            }

            return View("ShowAllCategories", await GetAllCategories());
        }

        /// <summary>
        /// Find category in database by id asynchronously
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        private async Task<Category> FindCategoryAsync(int? id)
        {
            using (var db = new DatabaseContext())
            {
                var currentCategory = await db.Categories.FindAsync(id);
                if (currentCategory != null)
                {
                    return currentCategory;
                }

                else
                {
                    throw new InvalidOperationException();
                }
            }
        }

        /// <summary>
        /// Find category in database and update category asynchronously
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        private async Task<bool> UpdateCurrentCategoryAsync(Category category)
        {
            using (var db = new DatabaseContext())
            {
                var currentCategory = await db.Categories.FirstOrDefaultAsync(c => c.Id == category.Id);

                if (currentCategory != null)
                {
                    currentCategory.Name = category.Name;
                    await db.SaveChangesAsync();

                    return true;
                }

                else
                {
                    return false;
                    throw new InvalidOperationException("Category Not Found");
                }
            }
        }

        public async Task<IActionResult> OnDeleteCategory(int? id)
        {
            ViewBag.IsDeleted = await DeleteCategory(id);

            return View("ShowAllCategories", await GetAllCategories());
        }

        private async Task<bool> DeleteCategory(int? id)
        {
            using (var db = new DatabaseContext())
            {
                var toRemove = await db.Categories.FirstOrDefaultAsync(c => c.Id == id);

                if (toRemove != null)
                {
                    db.Categories.Remove(toRemove);
                    await db.SaveChangesAsync();

                    return true;
                }
            }

            return false;
        }

        public IActionResult AddPost()
        {
            return View();
        }
        
        [HttpPost]
        public async Task<IActionResult> AddPost(Post post)
        {
            //if (ModelState.IsValid)
            //{
            try
            {
                await AddNewPost(post);
            }
        
            catch (Exception ex)
            {
                return View(ex.Message);
            }
            //}
        
            return View("Success"); //"Index", await GetAllPosts() 
        }
        
        private async Task AddNewPost(Post post)
        {
            using (var db = new DatabaseContext())
            {
                await db.Posts.AddAsync(
                    new Post
                    {
                        Title = post.Title,
                        Author = post.Author,
                        Date = post.Date,
                        Img = post.Img,
                        Description = post.Description,
                        Body = post.Body,
                        Category = new Category { Name = post.Category?.Name }
                    });
        
                await db.SaveChangesAsync();
            }
        }
    }
}