using CarBlogApp.Areas.Admin.Models;
using CarBlogApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
                var categoryExist = await db.Categories.FirstOrDefaultAsync(c => c.Name == category.Name);

                if (categoryExist == null)
                {
                    var addedCategory = await db.Categories.AddAsync(new Category { Name = category.Name?.Trim() });
                    await db.SaveChangesAsync();

                    return true;
                }

                return false;
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

                throw new InvalidOperationException();
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

                return false;
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

        public async Task<IActionResult> AddPost()
        {
            var categorySelectList = new SelectList(await GetAllCategories(), "Id", "Name");
            var model = new CreatePostViewModel
            {
                Post = new Post() { Date = DateTime.Today },
                Categories = categorySelectList
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AddPost(CreatePostViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                ViewBag.IsPostAdded = await AddNewPost(viewModel);
                return View("Index", await GetAllPosts());
            }

            return View(viewModel);
        }

        /// <summary>
        /// Asynchronously adds a new post to the database using the provided view model.
        /// </summary>
        /// <param name="viewModel"></param>
        /// <returns>Returns true if the post was successfully added; otherwise, returns false.</returns>
        private async Task<bool> AddNewPost(CreatePostViewModel viewModel)
        {
            string imagePath = await UploadPostImageAsync(viewModel);

            using (var db = new DatabaseContext())
            {
                if (viewModel.Post != null)
                {
                    var createPost = new Post
                    {
                        Title = viewModel.Post.Title,
                        Img = imagePath,
                        Description = viewModel.Post.Description,
                        Body = viewModel.Post.Body,
                        Author = viewModel.Post.Author,
                        Date = viewModel.Post.Date,
                        CategoryId = viewModel.Post.CategoryId
                    };

                    await db.Posts.AddAsync(createPost);
                    await db.SaveChangesAsync();

                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Uploads the post image asynchronously.
        /// </summary>
        /// <param name="viewModel">The view model containing the image file.</param>
        /// <returns>
        /// A string representing the path of the uploaded image if successful;
        /// Otherwise, returns a default image path.
        /// </returns>
        public async Task<string> UploadPostImageAsync(CreatePostViewModel viewModel)
        {
            if (viewModel.ImageFile != null && viewModel.ImageFile.Length > 0)
            {
                var uniqueImageFileName = Path.Combine(Guid.NewGuid().ToString() + viewModel.ImageFile.FileName);
                var uploadFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\uploads", uniqueImageFileName);
                var postImagePath = Path.Combine("../uploads/" + uniqueImageFileName);

                using (var fileStream = new FileStream(uploadFilePath, FileMode.Create))
                {
                    await viewModel.ImageFile.CopyToAsync(fileStream);
                }

                return $"{postImagePath}";
            }

            return $"../uploads/default.jpg";
        }

        public async Task<IActionResult> EditPost(int? id)
        {
            var model = new CreatePostViewModel
            {
                Post = await FindPostAsync(id),
                Categories = new SelectList(await GetAllCategories(), "Id", "Name")
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditPost(Post post)
        {
            if (ModelState.IsValid)
            {
                ViewBag.IsUpdated = await UpdatePostAsync(post);
            }

            return View("Index", await GetAllPosts());
        }
        /// <summary>
        /// Asynchronously finds and returns a Post with the specified ID from the database.
        /// </summary>
        /// <param name="id">The ID of the Post to find.</param>
        /// <returns>If a Post with the specified ID is found, returns the Post; otherwise, returns null.</returns>
        private async Task<Post?> FindPostAsync(int? id)
        {
            using (var db = new DatabaseContext())
            {
                var foundPost = await db.Posts.FindAsync(id);

                if (foundPost != null)
                {
                    return foundPost;
                }
            }

            return null;
        }

        /// <summary>
        /// Asynchronously updates a Post in the database with the information from the provided Post object.
        /// </summary>
        /// <param name="post">The Post object containing the updated information.</param>
        /// <returns>
        /// Returns true if the update is successful; otherwise, returns false.
        /// </returns>
        private async Task<bool> UpdatePostAsync(Post post)
        {
            using (var db = new DatabaseContext())
            {
                if (post == null)
                {
                    return false;
                }

                var foundPost = await db.Posts.FirstOrDefaultAsync(p => p.Id == post.Id);

                if (foundPost != null)
                {
                    foundPost.Title = post.Title;
                    foundPost.Img = post.Img;
                    foundPost.Description = post.Description;
                    foundPost.Body = post.Body;
                    foundPost.Author = post.Author;
                    foundPost.Date = post.Date;
                    foundPost.CategoryId = post.CategoryId;

                    await db.SaveChangesAsync();

                    return true;
                }

                return false;
            }
        }

        public async Task<IActionResult> DeletePost(int? id)
        {
            ViewBag.IsDeleted = await RemovePostAsync(id);

            return View("Index", await GetAllPosts());
        }

        /// <summary>
        /// Asynchronously removes a Post from the database based on its ID.
        /// </summary>
        /// <param name="id">The ID of the Post to be removed.</param>
        /// <returns>
        /// Returns true if the removal is successful; otherwise, returns false.
        /// </returns>
        private async Task<bool> RemovePostAsync(int? id)
        {
            using (var db = new DatabaseContext())
            {
                var foundPost = await db.Posts.FirstOrDefaultAsync(p => p.Id == id);

                if (foundPost != null)
                {
                    db.Posts.Remove(foundPost);
                    await db.SaveChangesAsync();

                    return true;
                }

                return false;
            }
        }
    }
}