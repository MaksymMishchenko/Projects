using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<MovieService> _logger;
        private readonly Random _random;
        public int PageSize = 1;

        public Task<int> CountAsync => _dbContext.Movies.CountAsync();

        public MovieService(ApplicationDbContext dbContext, ILogger<MovieService> logger, Random random)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _logger = logger;
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

            if (movies == null || !movies.Any())
            {
                throw new InvalidOperationException($"No movies available or couldn't retrieve movies from the database.");
            }

            return movies;
        }

        /// <summary>
        /// Asyncronously retrieves a paginated list of all movies from the database, including their genres.
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

        /// <summary>
        /// Retrieves a paginated list of movies from the database that match the provided search string in their title.
        /// It performs a case-insensitive search and includes the genre information for each movie.
        /// Logs search attempts, results, and exceptions. Throws exceptions for invalid input.
        /// </summary>
        /// <param name="searchString">The title or part of the title to search for. Cannot be null or empty.</param>
        /// <param name="moviePage">The page number for pagination. Must be greater than or equal to 1.</param>
        /// <returns>A tuple containing a list of matching movies and the total count of movies found for the search term.</returns>
        /// <exception cref="ArgumentException">Thrown when the search string is null or empty.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the movie page number is less than 1.</exception>
        /// <exception cref="Exception">Catches and logs any other unhandled exceptions that occur during the operation.</exception>
        public async Task<(List<Movie> Movies, int Count)> GetMoviesByTitleAsync(string searchString, int moviePage = 1)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchString))
                {
                    _logger.LogWarning($"Search string is null or empty in {nameof(GetMoviesByTitleAsync)}.");
                    throw new ArgumentException(nameof(searchString), "Search string in GetMoviesByTitleAsync cannot be null or empty.");
                }

                if (moviePage < 1)
                {
                    _logger.LogWarning($"Invalid movie page value {moviePage} in {nameof(GetMoviesByTitleAsync)}.");
                    throw new ArgumentOutOfRangeException(nameof(moviePage), "Movie page cannot be less than 1");
                }

                _logger.LogInformation($"Searching movies with title containing '{searchString}' on page {moviePage}.");

                var query = _dbContext.Movies
                    .AsNoTracking()
                    .Include(g => g.Genre)
                    .Where(m => m.Title != null && EF.Functions.Like(m.Title, $"%{searchString}%"));

                var movies = await query
                    .OrderBy(m => m.Id)
                    .Skip((moviePage - 1) * PageSize)
                    .Take(PageSize)
                    .ToListAsync();

                var totalCount = await query.CountAsync();

                if (!movies.Any())
                {
                    _logger.LogInformation($"No movies found for search term '{searchString}'.");
                    return (Movies: new List<Movie>(), Count: totalCount);
                }

                _logger.LogInformation($"{movies.Count} movies found for search term '{searchString}'.");
                return (Movies: movies, Count: totalCount);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in {nameof(GetMoviesByTitleAsync)}. Search string: {searchString}, Movie page: {moviePage}");
                throw;
            }
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

        /// <summary>
        /// Asynchronously retrieves a paginated list of movies by genre and the total count of movies in that genre.
        /// </summary>
        /// <param name="genre">The genre to search for.</param>
        /// <param name="moviePage">The page number for pagination. Default is 1.</param>
        /// <returns>A task representing the asynchronous operation, containing a tuple with a list of movies and the total count of movies in that genre.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the genre string is null or empty.</exception>
        /// <exception cref="KeyNotFoundException">Thrown if no movies are found for the specified genre.</exception>
        public async Task<(List<Movie> Movies, int Count)> GetMoviesByGenreAsync(string genre, int moviePage = 1)
        {
            if (string.IsNullOrEmpty(genre))
            {
                throw new ArgumentNullException(nameof(genre), "Search genre string is null or empty.");
            }

            var movieQuery = _dbContext.Movies.Include(m => m.Genre).Where(m => m.Genre.Name == genre);

            var totalCount = await movieQuery.CountAsync();

            var moviesByGenre = await _dbContext.Movies
                  .Include(m => m.Genre)
                  .Where(m => m.Genre.Name == genre)
                  .OrderBy(m => m.Id)
                  .Skip((moviePage - 1) * PageSize)
                  .Take(PageSize)
                  .ToListAsync();

            if (moviesByGenre == null || !moviesByGenre.Any())
            {
                throw new KeyNotFoundException($"Сouldn't find movies by genre: '{genre}'.");
            }

            return (Movies: moviesByGenre, Count: totalCount);
        }

        /// <summary>
        /// Asynchronously retrieves a distinct list of all genres from the movies in the database.
        /// </summary>
        /// <returns>A task representing the asynchronous operation, containing a list of unique <see cref="Genre"/> objects.</returns>
        /// <exception cref="InvalidOperationException">Thrown if no genres are found in the database.</exception>
        public async Task<List<Genre>> GetAllGenresAsync()
        {

            var movieGenres = await _dbContext.Movies
            .Include(m => m.Genre)
            .Select(m => m.Genre)
            .Distinct()
            .ToListAsync() ?? new List<Genre>();

            if (movieGenres == null || !movieGenres.Any())
            {
                throw new InvalidOperationException("No genres found");
            }

            return movieGenres;
        }

        /// <summary>
        /// Asynchronously updates the `IsFavorite` property of a movie in the database.
        /// If the movie with the specified ID does not exist, an `ArgumentNullException` is thrown.
        /// </summary>
        /// <param name="movieId">The ID of the movie to update.</param>
        /// <param name="isFavorite">The new favorite status to set for the movie.</param>
        /// <exception cref="ArgumentNullException">Thrown when the movie with the specified ID is not found in the database.</exception>      
        public async Task UpdateIsFavoriteAsync(int movieId, bool isFavorite)
        {
            var foundMovie = await _dbContext.Movies.FindAsync(movieId);

            //ArgumentNullException.ThrowIfNull(foundMovie);
            if (foundMovie == null)
            {
                throw new ArgumentNullException("An error occurred during finding the movie in database. Throw from MovieService in line 204");
            }

            foundMovie.IsFavorite = isFavorite;
            await _dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Asynchronously retrieves a list of all movies marked as favorites from the database, including their associated genres.
        /// Ensures that the result is not null by throwing an <see cref="ArgumentNullException"/> if the list is null.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result is a list of favorite movies with their associated genres.</returns>
        public async Task<(List<Movie> Movies, int Count)> GetListOfFavoriteMoviesAsync(int moviePage = 1)
        {
            var moviesByTrueFavoriteProperty = _dbContext.Movies
                .Include(m => m.Genre)
                .Where(m => m.IsFavorite == true);

            var count = await moviesByTrueFavoriteProperty.CountAsync();

            var favoriteMoviesList = await _dbContext.Movies
                .Include(m => m.Genre)
                .Where(m => m.IsFavorite == true)
                .OrderBy(m => m.Id)
                .Skip((moviePage - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            //ArgumentNullException.ThrowIfNull(favoriteMoviesList);
            if (favoriteMoviesList == null)
            {
                throw new ArgumentNullException("An error occurred during finding the movies in database. Throw from MovieService in line 231");
            }

            return (Movies: favoriteMoviesList, Count: count);
        }
    }
}