using CarBlogApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
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
            await AddPost();

            return View(await GetAllPosts());
        }

        private async Task<IEnumerable<Post>> GetAllPosts()
        {
            IEnumerable<Post> posts;
            using (var db = new DatabaseContext())
            {
                posts = await db.Posts.ToListAsync();
            }

            return posts;
        }

        public async Task<IActionResult> ShowFullPost(int id)
        {
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
        private async Task<Post> GetFullPost(int id)
        {
            Post? post = null;
            using (var db = new DatabaseContext())
            {
                post = await db.Posts.FindAsync(id);
            }

            return post!;
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
                         Body = "Lorem Ipsum is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the industry's standard dummy text ever since the 1500s, when an unknown printer took a galley of type and scrambled it to make a type specimen book. It has survived not only five centuries, but also the leap into electronic typesetting, remaining essentially unchanged. It was popularised in the 1960s with the release of Letraset sheets containing Lorem Ipsum passages, and more recently with desktop publishing software like Aldus PageMaker including versions of Lorem Ipsum."
                     },
                      new Post
                      {
                          Title = "Lorem ipsum dolor sit amet 2",
                          Author = "Peter",
                          Date = DateTime.Now,
                          Img = "../images/media-img.jpg",
                          Description = "This is body 2",
                          Body = "Lorem Ipsum is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the industry's standard dummy text ever since the 1500s, when an unknown printer took a galley of type and scrambled it to make a type specimen book. It has survived not only five centuries, but also the leap into electronic typesetting, remaining essentially unchanged. It was popularised in the 1960s with the release of Letraset sheets containing Lorem Ipsum passages, and more recently with desktop publishing software like Aldus PageMaker including versions of Lorem Ipsum."
                      },
                       new Post
                       {
                           Title = "Lorem ipsum dolor sit amet 3",
                           Author = "Peter",
                           Date = DateTime.Now,
                           Img = "../images/media-img.jpg",
                           Description = "this is body 3",
                           Body = "Lorem Ipsum is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the industry's standard dummy text ever since the 1500s, when an unknown printer took a galley of type and scrambled it to make a type specimen book. It has survived not only five centuries, but also the leap into electronic typesetting, remaining essentially unchanged. It was popularised in the 1960s with the release of Letraset sheets containing Lorem Ipsum passages, and more recently with desktop publishing software like Aldus PageMaker including versions of Lorem Ipsum."
                       });
                await db.SaveChangesAsync();
            }
        }

        public IActionResult About()
        {
            return View();
        }
        public IActionResult Contact()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Contact(ContactForm messages)
        {
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
        private async Task<ContactForm> AddMessageToInbox(ContactForm messages)
        {
            using (var db = new DatabaseContext())
            {
                db.InboxMessages.Add(messages);
                await db.SaveChangesAsync();
            }

            return messages;
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