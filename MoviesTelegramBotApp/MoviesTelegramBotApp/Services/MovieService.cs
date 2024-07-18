using Microsoft.EntityFrameworkCore;
using MoviesTelegramBotApp.Interfaces;
using MoviesTelegramBotApp.Models;
using System.Text;

namespace MoviesTelegramBotApp.Services
{
    internal class MovieService : IMovieService
    {
        private ApplicationDbContext _dbContext;
        public MovieService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task<List<Movie>> GetAllMoviesAsync()
        {
            return await _dbContext.Movies.Include(m => m.Genre).ToListAsync();
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
    }
}
