using CarBlogApp.Areas.Admin.Models;
using CarBlogApp.Models;

namespace CarBlogApp.Interfaces
{
    public interface IPostService
    {
        Task<IEnumerable<Post>> GetAllPostsAsync();       
        Task<IEnumerable<Post>> GetPostsByCategoryAsync(int? id);
        Task<bool> AddNewPostAsync(CreatePostViewModel viewModel);
        Task<bool> UpdatePostAsync(CreatePostViewModel viewModel);
        Task<bool> RemovePostAsync(int? id);
        Task<Post?> FindPostAsync(int? id);
    }
}
