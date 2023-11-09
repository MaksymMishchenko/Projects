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
                         Body = "This is body 1"
                     },
                      new Post
                      {
                          Title = "Lorem ipsum dolor sit amet 2",
                          Author = "Peter",
                          Date = DateTime.Now,
                          Img = "../images/media-img.jpg",
                          Body = "This is body 2"
                      },
                       new Post
                       {
                           Title = "Lorem ipsum dolor sit amet 3",
                           Author = "Peter",
                           Date = DateTime.Now,
                           Img = "../images/media-img.jpg",
                           Body = "This is body 3"
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

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}