using CarBlogApp.Models;

namespace CarBlogApp.Interfaces
{
    public interface ICategoryService
    {
        Task<IEnumerable<Category>> GetAllCategoriesAsync();
        Task<bool> CreateNewCategoryAsync(Category category);
        Task<bool> UpdateCurrentCategoryAsync(Category category);
        Task<bool> DeleteCategory(int? id);
        Task<Category?> FindCategoryAsync(int? id);
    }
}
