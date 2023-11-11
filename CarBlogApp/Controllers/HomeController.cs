using CarBlogApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace CarBlogApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            //await AddCategory(id);
            //await AddPost();
            var categoryModel = new CategoriesViewModel
            {
                Categories = await GetAllCategories()
            };

            ViewData["CategoriesViewModel"] = categoryModel;

            var posts = await GetAllPosts();

            return View(posts);
        }

        public async Task<IActionResult> ShowPostsByCategoryId(int? id)
        {
            var categoryModel = new CategoriesViewModel
            {
                Categories = await GetAllCategories()
            };

            ViewData["CategoriesViewModel"] = categoryModel;

            var postsByCategory = await GetPostsByCategoryId(id);

            return View(postsByCategory);
        }
        /// <summary>
        /// Get all categories from database
        /// </summary>
        /// <returns>List all post's categories</returns>
        private async Task<IEnumerable<Category>> GetAllCategories()
        {
            IEnumerable<Category> categories = new List<Category>();

            using (var db = new DatabaseContext())
            {
                categories = await db.Categories.ToListAsync();
            }

            return categories;
        }
        /// <summary>
        /// Get all posts from database
        /// </summary>
        /// <returns>List all post</returns>
        private async Task<IEnumerable<Post>> GetAllPosts()
        {
            IEnumerable<Post> posts = new List<Post>();

            using (var db = new DatabaseContext())
            {
                posts = await db.Posts.ToListAsync();
            }

            return posts;
        }

        public async Task<IActionResult> ShowFullPost(int? id)
        {
            var categoryModel = new CategoriesViewModel
            {
                Categories = await GetAllCategories()
            };

            ViewData["CategoriesViewModel"] = categoryModel;

            Post post = await GetFullPost(id);

            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }

        /// <summary>
        /// Search post in database asynchronously
        /// </summary>
        /// <param name="id">id of the current post</param>
        /// <returns>Found post from database</returns>
        private async Task<Post> GetFullPost(int? id)
        {
            Post? post = null;
            using (var db = new DatabaseContext())
            {
                post = await db.Posts.FindAsync(id);
            }

            return post!;
        }

        private async Task AddCategory(int? id)
        {
            var posts = await GetAllPosts();

            using (var db = new DatabaseContext())
            {
                db.Categories.AddRange(
                    new Category { Name = "Lamborgini", Posts = posts.Where(c => c.Category?.Id == id).ToList() },
                    new Category { Name = "Alfa-Romeo", Posts = posts.Where(c => c.Category?.Id == id).ToList() },
                    new Category { Name = "Mercedes", Posts = posts.Where(c => c.Category?.Id == id).ToList() },
                    new Category { Name = "Ferrari", Posts = posts.Where(c => c.Category?.Id == id).ToList() }
                    );

                await db.SaveChangesAsync();
            }
        }

        private async Task AddPost()
        {
            using (var db = new DatabaseContext())
            {
                db.Posts.AddRange(
                     new Post
                     {
                         Title = "Lorem ipsum dolor sit amet 1",
                         Author = "Peter",
                         Date = DateTime.Now,
                         Img = "../images/media-img.jpg",
                         Description = "This is body 1",
                         Body = "Lorem Ipsum is simply dummy text of the printing and typesetting industry." +
                          " Lorem Ipsum has been the industry's standard dummy text ever since the 1500s," +
                          " when an unknown printer took a galley of type and scrambled it to make a type specimen book." +
                          " It has survived not only five centuries, but also the leap into electronic typesetting," +
                          " remaining essentially unchanged. It was popularised in the 1960s with the release of Letraset sheets containing Lorem Ipsum passages," +
                          " and more recently with desktop publishing software like Aldus PageMaker including versions of Lorem Ipsum.",
                         CategoryId = 1
                     },
                      new Post
                      {
                          Title = "Lorem ipsum dolor sit amet 2",
                          Author = "Maks",
                          Date = DateTime.Now,
                          Img = "../images/media-img.jpg",
                          Description = "This is body 2",
                          Body = "Lorem Ipsum is simply dummy text of the printing and typesetting industry." +
                          " Lorem Ipsum has been the industry's standard dummy text ever since the 1500s," +
                          " when an unknown printer took a galley of type and scrambled it to make a type specimen book." +
                          " It has survived not only five centuries, but also the leap into electronic typesetting," +
                          " remaining essentially unchanged. It was popularised in the 1960s with the release of Letraset sheets containing Lorem Ipsum passages," +
                          " and more recently with desktop publishing software like Aldus PageMaker including versions of Lorem Ipsum.",
                          CategoryId = 2
                      },
                       new Post
                       {
                           Title = "Lorem ipsum dolor sit amet 3",
                           Author = "Ivan",
                           Date = DateTime.Now,
                           Img = "../images/media-img.jpg",
                           Description = "this is body 3",
                           Body = "Lorem Ipsum is simply dummy text of the printing and typesetting industry." +
                          " Lorem Ipsum has been the industry's standard dummy text ever since the 1500s," +
                          " when an unknown printer took a galley of type and scrambled it to make a type specimen book." +
                          " It has survived not only five centuries, but also the leap into electronic typesetting," +
                          " remaining essentially unchanged. It was popularised in the 1960s with the release of Letraset sheets containing Lorem Ipsum passages," +
                          " and more recently with desktop publishing software like Aldus PageMaker including versions of Lorem Ipsum.",
                           CategoryId = 3
                       },
                       new Post
                       {
                           Title = "Lorem ipsum dolor sit amet 3",
                           Author = "Ivan",
                           Date = DateTime.Now,
                           Img = "../images/media-img.jpg",
                           Description = "this is body 3",
                           Body = "Lorem Ipsum is simply dummy text of the printing and typesetting industry." +
                          " Lorem Ipsum has been the industry's standard dummy text ever since the 1500s," +
                          " when an unknown printer took a galley of type and scrambled it to make a type specimen book." +
                          " It has survived not only five centuries, but also the leap into electronic typesetting," +
                          " remaining essentially unchanged. It was popularised in the 1960s with the release of Letraset sheets containing Lorem Ipsum passages," +
                          " and more recently with desktop publishing software like Aldus PageMaker including versions of Lorem Ipsum.",
                           CategoryId = 4
                       }
                       );

                await db.SaveChangesAsync();
            }
        }
        /// <summary>
        /// Create posts list by category id 
        /// </summary>
        /// <param name="id">id which user pass choosing a category</param>
        /// <returns>all posts by category id</returns>
        private async Task<IEnumerable<Post>> GetPostsByCategoryId(int? id)
        {
            using (var db = new DatabaseContext())
            {
                return await db.Posts.Where(post => post.CategoryId == id).ToListAsync();
            }
        }

        public async Task<IActionResult> About()
        {
            var categoryModel = new CategoriesViewModel
            {
                Categories = await GetAllCategories()
            };

            ViewData["CategoriesViewModel"] = categoryModel;

            return View();
        }
        public async Task<IActionResult> Contact()
        {
            var categoryModel = new CategoriesViewModel
            {
                Categories = await GetAllCategories()
            };

            ViewData["CategoriesViewModel"] = categoryModel;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Contact(ContactForm messages)
        {
            var categoryModel = new CategoriesViewModel
            {
                Categories = await GetAllCategories()
            };

            ViewData["CategoriesViewModel"] = categoryModel;

            if (ModelState.IsValid)
            {
                await AddMessageToInbox(messages);
                return View("Success");
            }

            return View();
        }
        /// <summary>
        /// Add new message from contact form to database asynchronously
        /// </summary>
        /// <param name="messages"></param>
        /// <returns>Message added users in contact form</returns>
        private async Task<ContactForm> AddMessageToInbox(ContactForm message)
        {
            using (var db = new DatabaseContext())
            {
                db.InboxMessages.Add(message);
                await db.SaveChangesAsync();
            }

            return message;
        }

        public async Task<IActionResult> GetMagazine()
        {
            string path = "wwwroot\\files";
            string file = "sample.pdf";
            string contentType = "application/octet-stream";

            var memory = await DownloadFile(path, file);

            return File(memory, contentType, file);
        }
        /// <summary>
        /// Download file to MemoryStream asynchronously
        /// </summary>
        /// <param name="uploadPath"></param>
        /// <param name="fileName"></param>
        /// <returns>MemoryStream wich contains downloaded file</returns>
        private async Task<MemoryStream> DownloadFile(string uploadPath, string fileName)
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