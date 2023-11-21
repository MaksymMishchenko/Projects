using CarBlogApp.Areas.Admin.Models;
using CarBlogApp.Interfaces;
using CarBlogApp.Models;
using Microsoft.EntityFrameworkCore;

namespace CarBlogApp.Services
{
    public class PostService : IPostService, IDisposable
    {
        private readonly DatabaseContext? _dbContext;

        public PostService(DatabaseContext db)
        {
            _dbContext = db ?? throw new ArgumentNullException(nameof(db));
        }

        /// <summary>
        /// Retrieves all posts from the database, including related category information asynchronously.
        /// </summary>
        /// <returns>
        /// A collection of posts asynchronously
        /// </returns>
        public async Task<IEnumerable<Post>> GetAllPostsAsync()
        {
            if (_dbContext != null)
            {
                return await _dbContext.Posts.Include(c => c.Category).ToListAsync();
            }

            return Enumerable.Empty<Post>();
        }

        /// <summary>
        /// Retrieves a collection of posts associated with a specific category ID, including details of the related category asynchronously
        /// </summary>
        /// <param name="id">Id wich associated with specific category id</param>
        /// <returns>
        /// A collection of posts associated with a specific category ID
        /// </returns>
        public async Task<IEnumerable<Post>> GetPostsByCategoryAsync(int? id)
        {
            if (_dbContext != null)
            {
                return await _dbContext.Posts.Where(p => p.CategoryId == id).Include(p => p.Category).ToListAsync();
            }

            return Enumerable.Empty<Post>();
        }

        /// <summary>
        /// Method is responsible for adding a new post to the database based on the provided CreatePostViewModel
        /// </summary>
        /// <param name="viewModel"></param>
        /// <returns>
        /// True if the post is added successfully; otherwise, false.
        /// </returns>
        public async Task<bool> AddNewPostAsync(CreatePostViewModel viewModel)
        {
            if (_dbContext != null)
            {
                string imagePath = await UploadPostImageAsync(viewModel);

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

                    await _dbContext.Posts.AddAsync(createPost);
                    await _dbContext.SaveChangesAsync();

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Method is designed to handle the asynchronous upload of a post image
        /// </summary>
        /// <param name="viewModel">CreatePostViewModel</param>
        /// <returns>
        /// String which contains image path
        /// </returns>
        private static async Task<string> UploadPostImageAsync(CreatePostViewModel viewModel)
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

        /// <summary>
        /// Method is responsible for updating the current post
        /// </summary>
        /// <param name="viewModel"></param>
        /// <returns>
        /// True if the post is updated successfully; otherwise, false.
        /// </returns>
        public async Task<bool> UpdatePostAsync(CreatePostViewModel viewModel)
        {
            if (_dbContext != null)
            {
                var foundPost = await _dbContext.Posts.FirstOrDefaultAsync(p => p.Id == viewModel.Post!.Id);

                if (foundPost != null)
                {
                    foundPost.Title = viewModel.Post!.Title;
                    foundPost.Img = await UploadPostImageAsync(viewModel);
                    foundPost.Description = viewModel.Post.Description;
                    foundPost.Body = viewModel.Post.Body;
                    foundPost.Author = viewModel.Post.Author;
                    foundPost.Date = viewModel.Post.Date;
                    foundPost.CategoryId = viewModel.Post.CategoryId;

                    await _dbContext.SaveChangesAsync();

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Method is responsible for removing the post
        /// </summary>
        /// <param name="viewModel">post id</param>
        /// <returns>
        /// True if the post is deleted successfully; otherwise, false.
        /// </returns>
        public async Task<bool> RemovePostAsync(int? id)
        {
            if (_dbContext != null)
            {
                var foundPost = await _dbContext.Posts.FirstOrDefaultAsync(p => p.Id == id);

                if (foundPost != null)
                {
                    _dbContext.Posts.Remove(foundPost);
                    await _dbContext.SaveChangesAsync();

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Method is responsible for asynchronously locating a post based on the provided id
        /// </summary>
        /// <param name="id">Provided post id</param>
        /// <returns>
        /// Found post asynchronously
        /// </returns>
        public async Task<Post?> FindPostAsync(int? id)
        {
            if (_dbContext != null)
            {
                var foundPost = await _dbContext.Posts.FindAsync(id);

                if (foundPost != null)
                {
                    return foundPost;
                }
            }

            return null;
        }

        public void Dispose()
        {
            _dbContext?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
