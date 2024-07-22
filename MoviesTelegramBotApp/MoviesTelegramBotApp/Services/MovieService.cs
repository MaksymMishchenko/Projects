using Microsoft.EntityFrameworkCore;
using MoviesTelegramBotApp.Interfaces;
using MoviesTelegramBotApp.Models;
using System.Text;

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
            return await _dbContext.Movies.
                Include(m => m.Genre)
                .OrderBy(m => m.Id)
                .Skip((moviePage - 1) * PageSize)
                .Take(PageSize)
            .ToListAsync();
        }

        public string BuildMoviesResponse(List<Movie> movies)
        {
            var response = new StringBuilder();

            if (!movies.Any())
            {
                return "No movies found!";
            }

            foreach (var movie in movies)
            {
                response.AppendLine($"Title: {movie.Title}");
                response.AppendLine($"Description: {movie.Description}");
                response.AppendLine($"Country: {movie.Country}");
                response.AppendLine($"Budget: {movie.Budget}");
                response.AppendLine($"Genre: {movie.Genre}");
                response.AppendLine($"Trailer: {movie.MovieUrl}");
                response.AppendLine($"Interest facts: {movie.InterestFactsUrl}");
                response.AppendLine($"Behind the scene: {movie.BehindTheScene}");
                response.AppendLine();
            }

            return response.ToString();
        }

        public async Task<List<Movie>> FindMovieByTitleAsync(string searchString)
        {
            var foundMovie = await _dbContext.Movies.Include(g => g.Genre).Where(m => m.Title.Contains(searchString)).ToListAsync();

            if (foundMovie == null || !foundMovie.Any())
            {
                throw new KeyNotFoundException($"Movie with a key {searchString} does not found");
            }

            return foundMovie;
        }
    }
}
