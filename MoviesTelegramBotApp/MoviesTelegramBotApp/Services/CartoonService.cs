using Microsoft.EntityFrameworkCore;
using MoviesTelegramBotApp.Interfaces;
using MoviesTelegramBotApp.Models;

namespace MoviesTelegramBotApp.Services
{
    internal class CartoonService : ICartoonService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly int _pageSize = 1;
        public CartoonService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public Task<int> Count => _dbContext.Cartoons.CountAsync();

        public async Task<List<Cartoon>> GetAllCartoonsAsync(int cartoonPage = 1)
        {
            return await _dbContext.Cartoons
                .Include(c => c.Genre)
                .OrderBy(c => c.Id)
                .Skip((cartoonPage - 1) * _pageSize)
                .Take(_pageSize)
                .ToListAsync();
        }

        public async Task<Cartoon> GetCartoonById(int id)
        {
            var foundCartoon = await _dbContext.Cartoons.FindAsync(id);

            if (foundCartoon == null)
            {
                throw new KeyNotFoundException($"Cartoon with ID {id} not found.");
            }

            return foundCartoon;
        }

        public async Task<List<Cartoon>> GetCartoonsByGenre(string genre)
        {
            return await _dbContext.Cartoons
                .Include(c => c.Genre)
                .OrderBy(c => c.Id)
                .ToListAsync();
        }
    }
}
