using Microsoft.EntityFrameworkCore;
using MoviesTelegramBotApp.Interfaces;
using MoviesTelegramBotApp.Models;

namespace MoviesTelegramBotApp.Services
{
    /// <summary>
    /// Provides methods for retrieving and managing movie data.
    /// </summary>
    public class MovieService : IMovieService
    {
        private ApplicationDbContext _dbContext;
        private readonly Random _random;
        public int PageSize = 1;

        public Task<int> CountAsync => _dbContext.Movies.CountAsync();

        public MovieService(ApplicationDbContext dbContext, Random random)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _random = random;
        }

        /// <summary>
        /// Asynchronously retrieves all movies from the database, including their genres.
        /// </summary>
        /// <returns>A task representing the asynchronous operation, containing a list of <see cref="Movie"/> objects.</returns>
        /// <exception cref="InvalidOperationException">Thrown if no movies are available or if there was an issue retrieving movies from the database.</exception>
        private async Task<List<Movie>> GetAllMoviesAsync()
        {
            var movies = await _dbContext.Movies
                .Include(m => m.Genre)
                .ToListAsync();

            if (movies == null) //!movies.Any()
            {
                throw new InvalidOperationException($"No movies available or couldn't retrieve movies from the database.");
            }

            return movies;
        }

        /// <summary>
        /// Retrieves a paginated list of all movies from the database, including their genres.
        /// </summary>
        /// <param name="moviePage">The page number to retrieve. Defaults to 1.</param>
        /// <returns>A list of movies from the specified page.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the database connection fails or no movies are found.</exception>
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
                throw new InvalidOperationException($"No movies available or couldn't retrieve movies from the database.");
            }

            return movies;
        }

        // <summary>
        /// Retrieves a paginated list of movies whose titles contain the specified search string.
        /// </summary>
        /// <param name="searchString">The string to search for within movie titles.</param>
        /// <param name="moviePage">The page number to retrieve. Defaults to 1.</param>
        /// <returns>A list of movies that contain the search string in their titles.</returns>
        /// <exception cref="KeyNotFoundException">Thrown when no movies are found matching the search string.</exception>
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

        /// <summary>
        /// Asynchronously gets the count of movies that contain the specified title substring.
        /// </summary>
        /// <param name="searchString">The substring to search for in movie titles.</param>
        /// <returns>The count of movies whose titles contain the specified substring.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the search string is null or empty.</exception>
        /// <remarks>
        /// This method checks if the search string is null or empty and throws an ArgumentNullException if it is. 
        /// It then queries the database to count the number of movies whose titles contain the search string.
        /// </remarks>
        public async Task<int> GetMoviesByTitleCountAsync(string searchString)
        {
            if (string.IsNullOrEmpty(searchString))
            {
                throw new ArgumentNullException(nameof(searchString), "Search string cannot be null or empty.");
            }

            return await _dbContext.Movies
            .Where(m => m.Title!.Contains(searchString))
            .CountAsync();
        }

        /// <summary>
        /// Asynchronously retrieves a random movie from the collection of all movies.
        /// </summary>
        /// <returns>A task representing the asynchronous operation, containing a randomly selected <see cref="Movie"/> object.</returns>
        /// <exception cref="InvalidOperationException">Thrown if no movies are available in the collection.</exception>
        public async Task<Movie> GetRandomMovieAsync()
        {
            try
            {
                var movies = await GetAllMoviesAsync();
                int randomIndex = _random.Next(1, movies.Count);

                return movies[randomIndex];
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
