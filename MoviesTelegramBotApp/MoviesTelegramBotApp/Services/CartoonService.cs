using MoviesTelegramBotApp.Interfaces;
using MoviesTelegramBotApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoviesTelegramBotApp.Services
{
    internal class CartoonService : ICartoonService
    {
        private readonly ApplicationDbContext _dbContext;
        public CartoonService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public Task<int> Count => throw new NotImplementedException();

        public Task<List<Cartoon>> GetAllCartoonsAsync(int cartoonPage)
        {
            throw new NotImplementedException();
        }

        public Task<List<Cartoon>> GetCartoonsByGenre(string genre)
        {
            throw new NotImplementedException();
        }
    }
}
