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
        public int PageSize = 1;

        public Task<int> Count => _dbContext.Movies.CountAsync();

        public MovieService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        /// <summary>
        /// Retrieves a paginated list of all movies from the database, including their genres.
        /// </summary>
        /// <param name="moviePage">The page number to retrieve. Defaults to 1.</param>
        /// <returns>A list of movies from the specified page.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the database connection fails or no movies are found.</exception>
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
        /// Retrieves a movie by it's ID
        /// </summary>
        /// <param name="id">The Id of the movie to retrieve</param>
        /// <returns>The movie with specified ID, if found</returns>
        /// <exception cref="NullReferenceException">Thrown when no movie with the specified ID is found</exception>
        public async Task<Movie> GetMovieById(int id)
        {
            var movie = await _dbContext.Movies.FindAsync(id);

            if (movie == null)
            {
                throw new NullReferenceException($"Movie with {id} was not found");
            }

            return movie;
        }
    }
}
