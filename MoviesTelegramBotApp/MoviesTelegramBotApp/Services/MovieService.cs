using Microsoft.EntityFrameworkCore;
using MoviesTelegramBotApp.Interfaces;
using MoviesTelegramBotApp.Models;

namespace MoviesTelegramBotApp.Services
{
    public class MovieService : IMovieService
    {
        private ApplicationDbContext _dbContext;
        public int PageSize = 1;

        public Task<int> Count => _dbContext.Movies.CountAsync();

        public MovieService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task<List<Movie>> GetAllMoviesAsync(int moviePage = 1)
        {
            var movies = await _dbContext.Movies.
                Include(m => m.Genre)
                .OrderBy(m => m.Id)
                .Skip((moviePage - 1) * PageSize)
                .Take(PageSize)
            .ToListAsync();

            if (movies == null || !movies.Any())
            {
                throw new ArgumentNullException($"Couldn't connect to database");
            }

            return movies;
        }

        public async Task<List<Movie>> GetMoviesByTitleAsync(string searchString, int moviePage = 1)
        {
            var foundMovie = await _dbContext.Movies
                .Include(g => g.Genre)
                .Where(m => m.Title!.Contains(searchString))
                .OrderBy(m => m.Id)
                .Skip((moviePage - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            if (foundMovie == null || !foundMovie.Any())
            {
                throw new KeyNotFoundException($"We couldn't find '{searchString}'.\n Would you like to try searching with a different title?");
            }

            return foundMovie;
        }

        public async Task<int> GetMoviesByTitleCountAsync(string searchString)
        {
            return await _dbContext.Movies
            .Where(m => m.Title!.Contains(searchString))
            .CountAsync();
        }
    }
}
