using CarBlogApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;

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
            return View(await GetAllCategories());
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
    }
}