using CarBlogApp.Interfaces;
using CarBlogApp.Models;
using Microsoft.EntityFrameworkCore;

namespace CarBlogApp.Services
{
    public class CategoryService : ICategoryService, IDisposable
    {
        private readonly DatabaseContext _dbContext;

        public CategoryService(DatabaseContext db)
        {
            _dbContext = db;
        }

        /// <summary>
        /// Retrieves all categories asynchronously from the database.
        /// </summary>
        /// <returns>
        /// A collection of categories if available; otherwise, an empty collection.
        /// </returns>
        public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
        {            
            if (_dbContext != null)
            {
                return await _dbContext.Categories.ToListAsync();
            }
            
            return Enumerable.Empty<Category>();
        }

        /// <summary>
        /// Creates a new category asynchronously in the database if it does not already exist.
        /// </summary>
        /// <param name="category">The category to be created.</param>
        /// <returns>
        /// True if the category is created successfully; otherwise, false.
        /// </returns>
        public async Task<bool> CreateNewCategoryAsync(Category category)
        {
            if (_dbContext != null)
            {
                var categoryExist = await _dbContext.Categories.FirstOrDefaultAsync(c => c.Name == category.Name);

                if (categoryExist == null)
                {
                    var addedCategory = await _dbContext.Categories.AddAsync(new Category { Name = category.Name?.Trim() });
                    await _dbContext.SaveChangesAsync();

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Updates the information of the specified category asynchronously in the database.
        /// </summary>
        /// <param name="category">The category with updated information.</param>
        /// <returns>
        /// True if the category is updated successfully; otherwise, false.
        /// </returns>
        public async Task<bool> UpdateCurrentCategoryAsync(Category category)
        {
            if (_dbContext != null)
            {
                var currentCategory = await _dbContext.Categories.FirstOrDefaultAsync(c => c.Id == category.Id);

                if (currentCategory != null)
                {
                    currentCategory.Name = category.Name;
                    await _dbContext.SaveChangesAsync();

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Deletes the specified category asynchronously from the database.
        /// </summary>
        /// <param name="id">The ID of the category to be deleted.</param>
        /// <returns>
        /// True if the category is deleted successfully; otherwise, false.
        /// </returns>
        public async Task<bool> DeleteCategory(int? id)
        {
            if (_dbContext != null)
            {
                var toRemove = await _dbContext.Categories.FirstOrDefaultAsync(c => c.Id == id);

                if (toRemove != null)
                {
                    _dbContext.Categories.Remove(toRemove);
                    await _dbContext.SaveChangesAsync();

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Finds and returns the category with the specified ID asynchronously.
        /// </summary>
        /// <param name="id">The ID of the category to be found.</param>
        /// <returns>
        /// The category with the specified ID if found; otherwise, null.
        /// </returns>
        public async Task<Category?> FindCategoryAsync(int? id)
        {
            if (_dbContext != null && id != null)
            {
                return await _dbContext.Categories.FindAsync(id);
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
